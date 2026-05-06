using System.Net.Http.Json;
using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

/// <summary>
/// Real HTTP implementation of <see cref="IInventoryClient"/> that communicates with
/// Inventory.Service via Aspire service discovery.
///
/// Single-call flow:
///   POST /api/inventory/deduct → returns { success, unitPrice, message }
///
/// If the call fails the method returns a failed <see cref="InventoryDeductResult"/>
/// instead of throwing, allowing <see cref="Order.Application.Services.OrderService"/>
/// to surface a 409 Conflict to the caller.
/// </summary>
public class HttpInventoryClient : IInventoryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpInventoryClient> _logger;

    public HttpInventoryClient(IHttpClientFactory httpClientFactory, ILogger<HttpInventoryClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<InventoryDeductResult> DeductStockAsync(
        Guid productId, int quantity, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Inventory");

        // POST /api/inventory/deduct accepts ChipBakery.Shared.OrderRequest.
        // CustomerName and CustomerId are not used by the deduct endpoint — it maps only ProductId + Quantity.
        var deductRequest = new OrderRequest(productId, quantity, string.Empty, string.Empty);

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/inventory/deduct", deductRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network error calling /api/inventory/deduct for product {ProductId}", productId);
            return new InventoryDeductResult(false, 0,
                "Could not reach Inventory.Service. Please try again later.");
        }

        // Both 200 (success) and 400 (failure) carry an InventoryDeductResult body.
        try
        {
            var result = await response.Content.ReadFromJsonAsync<InventoryDeductResult>(JsonOptions, ct);

            if (result is null)
            {
                _logger.LogWarning(
                    "Inventory.Service returned an empty body for product {ProductId} (HTTP {Status}).",
                    productId, (int)response.StatusCode);
                return new InventoryDeductResult(false, 0,
                    $"Inventory.Service returned an empty response (HTTP {(int)response.StatusCode}).");
            }

            if (result.Success)
            {
                _logger.LogInformation(
                    "Inventory deduction succeeded — product {ProductId} x{Quantity} @ {UnitPrice:C} each",
                    productId, quantity, result.UnitPrice);
            }
            else
            {
                _logger.LogWarning(
                    "Inventory deduction failed for product {ProductId} x{Quantity}: {Error}",
                    productId, quantity, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse /api/inventory/deduct response for product {ProductId}", productId);
            return new InventoryDeductResult(false, 0,
                $"Failed to parse Inventory.Service response (HTTP {(int)response.StatusCode}).");
        }
    }
}

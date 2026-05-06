using System.Net;
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
/// Flow for each <see cref="DeductStockAsync"/> call:
///   1. GET  /api/inventory/{productId}  → fetch current unit price
///   2. POST /api/inventory/deduct       → atomically deduct stock
///
/// If either call fails the method returns a failed <see cref="InventoryDeductResult"/>
/// instead of throwing, allowing <see cref="Order.Application.Services.OrderService"/>
/// to surface a 409 Conflict to the caller.
/// </summary>
public class HttpInventoryClient : IInventoryClient
{
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

        // ── Step 1: Retrieve the inventory item to get the current unit price ────────
        // GET /api/inventory/{id} returns the internal InventoryItemDto JSON shape:
        //   { "id": "...", "name": "...", "price": 3.50, "quantity": 100 }
        HttpResponseMessage itemResponse;
        try
        {
            itemResponse = await client.GetAsync($"/api/inventory/{productId}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network error reaching Inventory.Service for product {ProductId}", productId);
            return new InventoryDeductResult(false, 0,
                "Could not reach Inventory.Service. Please try again later.");
        }

        if (itemResponse.StatusCode == HttpStatusCode.NotFound)
            return new InventoryDeductResult(false, 0,
                $"Product '{productId}' was not found in inventory.");

        if (!itemResponse.IsSuccessStatusCode)
            return new InventoryDeductResult(false, 0,
                $"Inventory.Service returned an unexpected status ({(int)itemResponse.StatusCode}) while fetching product.");

        InventoryItemResponse? item;
        try
        {
            item = await itemResponse.Content.ReadFromJsonAsync<InventoryItemResponse>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialise inventory item response for product {ProductId}", productId);
            return new InventoryDeductResult(false, 0, "Failed to parse product data from Inventory.Service.");
        }

        if (item is null)
            return new InventoryDeductResult(false, 0, "Received empty product data from Inventory.Service.");

        // ── Step 2: Deduct the stock ─────────────────────────────────────────────────
        // POST /api/inventory/deduct accepts ChipBakery.Shared.OrderRequest.
        // CustomerName and CustomerId are not used by the deduct endpoint — it maps only ProductId + Quantity.
        var deductRequest = new OrderRequest(productId, quantity, string.Empty, string.Empty);

        HttpResponseMessage deductResponse;
        try
        {
            deductResponse = await client.PostAsJsonAsync("/api/inventory/deduct", deductRequest, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network error calling /api/inventory/deduct for product {ProductId}", productId);
            return new InventoryDeductResult(false, 0,
                "Could not reach Inventory.Service during deduction. Please try again later.");
        }

        if (!deductResponse.IsSuccessStatusCode)
        {
            var error = await TryReadProblemDetailAsync(deductResponse, ct);
            _logger.LogWarning(
                "Inventory deduction failed for product {ProductId} x{Quantity}: {Error}",
                productId, quantity, error);
            return new InventoryDeductResult(false, 0, error);
        }

        _logger.LogInformation(
            "Inventory deduction succeeded — product {ProductId} x{Quantity} @ {UnitPrice:C} each",
            productId, quantity, item.Price);

        return new InventoryDeductResult(true, item.Price);
    }

    /// <summary>
    /// Attempts to extract the RFC 7807 <c>detail</c> field from a ProblemDetails response.
    /// Falls back to a generic message if parsing fails.
    /// </summary>
    private static async Task<string> TryReadProblemDetailAsync(
        HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? "Inventory deduction failed.";
        }
        catch
        {
            // Swallow parse failures — return generic message below
        }

        return $"Inventory deduction failed (HTTP {(int)response.StatusCode}).";
    }

    // ── Local deserialization shape ───────────────────────────────────────────────
    // Matches the JSON emitted by Inventory.Service GET /api/inventory/{id}
    // (InventoryItemDto: { id, name, price, quantity }).
    // Defined privately here so Infrastructure has no compile-time dep on Inventory internals.
    private sealed record InventoryItemResponse(Guid Id, string Name, decimal Price, int Quantity);
}

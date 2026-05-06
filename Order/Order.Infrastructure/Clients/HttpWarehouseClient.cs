using System.Net.Http.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

public class HttpWarehouseClient : IWarehouseClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpWarehouseClient> _logger;

    public HttpWarehouseClient(IHttpClientFactory httpClientFactory, ILogger<HttpWarehouseClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<RecipeCheckResponse> CheckRecipeAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Warehouse");

        try
        {
            var request = new RecipeCheckRequest(productId, quantity);
            var response = await client.PostAsJsonAsync("/api/warehouse/check-recipe", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                return new RecipeCheckResponse(false, $"Warehouse.Service returned {(int)response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<RecipeCheckResponse>(ct)
                ?? new RecipeCheckResponse(false, "Failed to parse response from Warehouse.Service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check recipe in Warehouse.Service for product {ProductId}", productId);
            return new RecipeCheckResponse(false, "Warehouse.Service is currently unavailable.");
        }
    }
}

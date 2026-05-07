using System.Net.Http.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;
using Production.Application.Interfaces;

namespace Production.Infrastructure.Clients;

public class HttpWarehouseClient(IHttpClientFactory httpClientFactory, ILogger<HttpWarehouseClient> logger) : IWarehouseClient
{
    public async Task<ConsumeRecipeResponse> ConsumeRecipeAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("Warehouse");

        try
        {
            var request = new ConsumeRecipeRequest(productId, quantity);
            var response = await client.PostAsJsonAsync("/api/warehouse/consume-recipe", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                return new ConsumeRecipeResponse(
                    Consumed: false,
                    Message: $"Warehouse.Service returned {(int)response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<ConsumeRecipeResponse>(ct)
                ?? new ConsumeRecipeResponse(false, Message: "Failed to parse response from Warehouse.Service");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to consume recipe in Warehouse.Service for product {ProductId}", productId);
            return new ConsumeRecipeResponse(false, Message: "Warehouse.Service is currently unavailable.");
        }
    }
}

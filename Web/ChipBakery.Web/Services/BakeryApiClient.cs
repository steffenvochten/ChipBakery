using System.Net.Http.Json;
using System.Text.Json;
using ChipBakery.Shared;

namespace ChipBakery.Web.Services;

public class BakeryApiClient(IHttpClientFactory httpClientFactory)
{
    // ─── Products & Storefront ──────────────────────────────────────────────

    public async Task<List<ProductItem>> GetAvailableProductsAsync()
    {
        var client = httpClientFactory.CreateClient("Inventory");
        return await client.GetFromJsonAsync<List<ProductItem>>("/api/inventory/available") ?? new();
    }

    // ─── Orders ─────────────────────────────────────────────────────────────

    public async Task<List<OrderItem>> GetAllOrdersAsync()
    {
        var client = httpClientFactory.CreateClient("Order");
        return await client.GetFromJsonAsync<List<OrderItem>>("/api/orders") ?? new();
    }

    public async Task<OrderResponse> PlaceOrderAsync(OrderRequest request)
    {
        var client = httpClientFactory.CreateClient("Order");
        var response = await client.PostAsJsonAsync("/api/orders", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrderResponse>()
                ?? new OrderResponse(false, "Unknown error");
        }

        try
        {
            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return new OrderResponse(false, detail.GetString() ?? "Order failed.");
            if (doc.RootElement.TryGetProperty("message", out var message))
                return new OrderResponse(false, message.GetString() ?? "Order failed.");
        }
        catch { /* ignore */ }

        return new OrderResponse(false, $"Order failed with status: {response.StatusCode}");
    }

    public async Task<bool> CancelOrderAsync(Guid id)
    {
        var client = httpClientFactory.CreateClient("Order");
        var response = await client.PutAsync($"/api/orders/{id}/cancel", null);
        return response.IsSuccessStatusCode;
    }

    // ─── Inventory Management ───────────────────────────────────────────────

    public async Task<List<InventoryItem>> GetAllInventoryAsync()
    {
        var client = httpClientFactory.CreateClient("Inventory");
        return await client.GetFromJsonAsync<List<InventoryItem>>("/api/inventory") ?? new();
    }

    public async Task<InventoryItem?> CreateInventoryItemAsync(CreateInventoryRequest request)
    {
        var client = httpClientFactory.CreateClient("Inventory");
        var response = await client.PostAsJsonAsync("/api/inventory", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InventoryItem>();
        }
        return null; // Should handle problem details ideally, keeping simple for now
    }

    public async Task<InventoryItem?> UpdateInventoryItemAsync(Guid id, UpdateInventoryRequest request)
    {
        var client = httpClientFactory.CreateClient("Inventory");
        var response = await client.PutAsJsonAsync($"/api/inventory/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<InventoryItem>();
        }
        return null;
    }

    public async Task<bool> DeleteInventoryItemAsync(Guid id)
    {
        var client = httpClientFactory.CreateClient("Inventory");
        var response = await client.DeleteAsync($"/api/inventory/{id}");
        return response.IsSuccessStatusCode;
    }

    // ─── Warehouse ──────────────────────────────────────────────────────────

    public async Task<List<WarehouseItem>> GetAllWarehouseItemsAsync()
    {
        var client = httpClientFactory.CreateClient("Warehouse");
        return await client.GetFromJsonAsync<List<WarehouseItem>>("/api/warehouse") ?? new();
    }

    public async Task<bool> CreateWarehouseItemAsync(CreateWarehouseItemRequest request)
    {
        var client = httpClientFactory.CreateClient("Warehouse");
        var response = await client.PostAsJsonAsync("/api/warehouse", request);
        return response.IsSuccessStatusCode;
    }

    // ─── Production ─────────────────────────────────────────────────────────

    public async Task<List<BakingJob>> GetAllBakingJobsAsync()
    {
        var client = httpClientFactory.CreateClient("Production");
        return await client.GetFromJsonAsync<List<BakingJob>>("/api/production") ?? new();
    }

    // ─── Loyalty ────────────────────────────────────────────────────────────

    public async Task<CustomerLoyalty?> GetCustomerLoyaltyAsync(Guid customerId)
    {
        var client = httpClientFactory.CreateClient("Loyalty");
        var response = await client.GetAsync($"/api/loyalty/{customerId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CustomerLoyalty>();
        }
        return null;
    }
}

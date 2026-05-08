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

    public async Task<List<RecipeDto>> GetAllRecipesAsync()
    {
        var client = httpClientFactory.CreateClient("Warehouse");
        return await client.GetFromJsonAsync<List<RecipeDto>>("/api/warehouse/recipes") ?? new();
    }

    public async Task<RecipeDto?> UpsertRecipeAsync(CreateRecipeRequest request)
    {
        var client = httpClientFactory.CreateClient("Warehouse");
        var response = await client.PostAsJsonAsync("/api/warehouse/recipes", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecipeDto>();
    }

    public async Task<bool> DeleteRecipeAsync(Guid productId)
    {
        var client = httpClientFactory.CreateClient("Warehouse");
        var response = await client.DeleteAsync($"/api/warehouse/recipes/{productId}");
        return response.IsSuccessStatusCode;
    }

    // ─── Production ─────────────────────────────────────────────────────────

    public async Task<List<BakingJob>> GetAllBakingJobsAsync()
    {
        var client = httpClientFactory.CreateClient("Production");
        return await client.GetFromJsonAsync<List<BakingJob>>("/api/production") ?? new();
    }

    // ─── Supplier ───────────────────────────────────────────────────────────

    public async Task<List<SupplierTransportDto>> GetAllSupplierTransportsAsync()
    {
        var client = httpClientFactory.CreateClient("Supplier");
        return await client.GetFromJsonAsync<List<SupplierTransportDto>>("/api/supplier") ?? new();
    }

    public async Task<bool> DispatchTransportAsync(DispatchTransportRequest request)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.PostAsJsonAsync("/api/supplier/dispatch", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<IngredientSupplyDto>> GetAllIngredientSuppliesAsync()
    {
        var client = httpClientFactory.CreateClient("Supplier");
        return await client.GetFromJsonAsync<List<IngredientSupplyDto>>("/api/supplier/ingredients") ?? new();
    }

    public async Task<IngredientSupplyDto?> GetIngredientSupplyByIdAsync(Guid id)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.GetAsync($"/api/supplier/ingredients/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IngredientSupplyDto>();
        }
        return null;
    }

    public async Task<IngredientSupplyDto> CreateIngredientSupplyAsync(CreateIngredientSupplyRequest req)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.PostAsJsonAsync("/api/supplier/ingredients", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IngredientSupplyDto>())
            ?? throw new InvalidOperationException("Empty response from server.");
    }

    public async Task<IngredientSupplyDto> UpdateIngredientSupplyAsync(Guid id, UpdateIngredientSupplyRequest req)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.PutAsJsonAsync($"/api/supplier/ingredients/{id}", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IngredientSupplyDto>())
            ?? throw new InvalidOperationException("Empty response from server.");
    }

    public async Task DeleteIngredientSupplyAsync(Guid id)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.DeleteAsync($"/api/supplier/ingredients/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<SupplierTransportDto> DispatchRestockAsync(RestockRequest req)
    {
        var client = httpClientFactory.CreateClient("Supplier");
        var response = await client.PostAsJsonAsync("/api/supplier/restock", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SupplierTransportDto>())
            ?? throw new InvalidOperationException("Empty response from server.");
    }

    // ─── Catalog (Orchestration) ───────────────────────────────────────────

    public async Task<RecipeOrchestrationResponse> OrchestrateRecipeCreationAsync(CreateRecipeOrchestrationRequest request)
    {
        var client = httpClientFactory.CreateClient("Catalog");
        var response = await client.PostAsJsonAsync("/api/catalog/recipes", request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RecipeOrchestrationResponse>()
                ?? new RecipeOrchestrationResponse(false, "Unknown error", null);
        }

        return new RecipeOrchestrationResponse(false, $"Orchestration failed with status: {response.StatusCode}", null);
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

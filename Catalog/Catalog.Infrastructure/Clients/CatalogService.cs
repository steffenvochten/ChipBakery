using System.Net.Http;
using System.Net.Http.Json;
using Catalog.Application.Interfaces;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Clients;

public class CatalogService(
    IHttpClientFactory httpClientFactory,
    ILogger<CatalogService> logger) : ICatalogService
{
    public async Task<RecipeOrchestrationResponse> OrchestrateRecipeCreationAsync(CreateRecipeOrchestrationRequest request, CancellationToken ct)
    {
        try
        {
            var warehouseClient = httpClientFactory.CreateClient("Warehouse");
            var inventoryClient = httpClientFactory.CreateClient("Inventory");
            var agentsClient = httpClientFactory.CreateClient("Agents");

            // 1. Ensure ingredients exist in Warehouse
            var newIngredientNames = new List<string>();
            foreach (var ing in request.Ingredients)
            {
                // Simple approach: try to create. Warehouse service should handle duplicates or we check first.
                // For this implementation, we'll assume the Warehouse handles "upsert" or we just POST and ignore 409/Conflict if it exists.
                // Better: Warehouse has a GET /api/warehouse. We check names.
                
                var existingItems = await warehouseClient.GetFromJsonAsync<List<WarehouseItem>>("/api/warehouse", ct) ?? [];
                if (!existingItems.Any(i => i.Name.Equals(ing.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    logger.LogInformation("Creating missing ingredient: {Ingredient}", ing.Name);
                    var createIngReq = new CreateWarehouseItemRequest 
                    { 
                        Name = ing.Name, 
                        Quantity = 0, // Start with zero stock
                        Unit = ing.Unit 
                    };
                    await warehouseClient.PostAsJsonAsync("/api/warehouse", createIngReq, ct);
                    newIngredientNames.Add(ing.Name);
                }
            }

            // 2. Create Product in Inventory
            logger.LogInformation("Creating product in Inventory: {Product}", request.ProductName);
            var createInvReq = new CreateInventoryRequest(request.ProductName, request.ProductPrice, 0);
            var invResp = await inventoryClient.PostAsJsonAsync("/api/inventory", createInvReq, ct);
            
            if (!invResp.IsSuccessStatusCode)
            {
                return new RecipeOrchestrationResponse(false, $"Failed to create product in Inventory: {invResp.ReasonPhrase}", null);
            }

            var createdProduct = await invResp.Content.ReadFromJsonAsync<InventoryItem>(ct);
            if (createdProduct == null)
            {
                return new RecipeOrchestrationResponse(false, "Failed to parse created product from Inventory.", null);
            }

            // 3. Create Recipe in Warehouse
            logger.LogInformation("Creating recipe for product {ProductId} in Warehouse", createdProduct.Id);
            var recipeReq = new CreateRecipeRequest(
                createdProduct.Id,
                createdProduct.Name,
                request.Ingredients.Select(i => new CreateRecipeIngredientRequest(i.Name, i.Quantity, i.Unit)).ToList());
            
            var recipeResp = await warehouseClient.PostAsJsonAsync("/api/warehouse/recipes", recipeReq, ct);
            if (!recipeResp.IsSuccessStatusCode)
            {
                return new RecipeOrchestrationResponse(false, $"Failed to create recipe in Warehouse: {recipeResp.ReasonPhrase}", createdProduct.Id);
            }

            // 4. Request new Supplier Agents for new ingredients
            if (newIngredientNames.Count > 0)
            {
                logger.LogInformation("Requesting auto-generated supplier for: {Ingredients}", string.Join(", ", newIngredientNames));
                // We'll group them into one supplier for simplicity or one per ingredient? 
                // Let's do one supplier that handles all new ingredients from this recipe.
                var agentReq = new { Ingredients = newIngredientNames };
                await agentsClient.PostAsJsonAsync("/api/agents/suppliers/auto", agentReq, ct);
            }

            return new RecipeOrchestrationResponse(true, "Successfully orchestrated recipe creation.", createdProduct.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during recipe orchestration for {Product}", request.ProductName);
            return new RecipeOrchestrationResponse(false, $"An error occurred: {ex.Message}", null);
        }
    }
}

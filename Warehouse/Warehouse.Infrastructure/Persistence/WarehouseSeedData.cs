using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Persistence;

public static class WarehouseSeedData
{
    public static async Task SeedAsync(WarehouseDbContext db, CancellationToken ct = default)
    {
        if (await db.Recipes.AnyAsync(ct)) return;

        var breadRecipeId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
        var cakeRecipeId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

        var bread = new Recipe
        {
            Id = breadRecipeId,
            ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ProductName = "Sourdough Bread",
            Ingredients = new List<RecipeIngredient>
            {
                new() { Id = Guid.NewGuid(), RecipeId = breadRecipeId, IngredientName = "Bread Flour", QuantityRequired = 0.5m, Unit = "kg" },
                new() { Id = Guid.NewGuid(), RecipeId = breadRecipeId, IngredientName = "Yeast", QuantityRequired = 0.01m, Unit = "kg" }
            }
        };

        var cake = new Recipe
        {
            Id = cakeRecipeId,
            ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ProductName = "Butter Cake",
            Ingredients = new List<RecipeIngredient>
            {
                new() { Id = Guid.NewGuid(), RecipeId = cakeRecipeId, IngredientName = "Bread Flour", QuantityRequired = 0.3m, Unit = "kg" },
                new() { Id = Guid.NewGuid(), RecipeId = cakeRecipeId, IngredientName = "Butter", QuantityRequired = 0.2m, Unit = "kg" },
                new() { Id = Guid.NewGuid(), RecipeId = cakeRecipeId, IngredientName = "Milk", QuantityRequired = 0.25m, Unit = "liters" }
            }
        };

        await db.Recipes.AddRangeAsync(new[] { bread, cake }, ct);
        await db.SaveChangesAsync(ct);
    }
}

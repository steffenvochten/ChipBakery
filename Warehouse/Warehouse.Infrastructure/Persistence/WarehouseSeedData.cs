using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Persistence;

public static class WarehouseSeedData
{
    // These match the stable GUIDs seeded by Inventory.Infrastructure InventoryItemConfiguration.
    private static readonly Guid SourdoughLoafId   = Guid.Parse("f2e1a3b4-64df-41ca-87ab-51b1f2e2ac89");
    private static readonly Guid ButterCroissantId = Guid.Parse("d3b4e7e4-23fb-47df-bc6c-18a0ea12cb24");
    private static readonly Guid ChocolateEclairId = Guid.Parse("e8a21d5a-1c7c-47db-93ab-62c1e7f3d91b");

    public static async Task SeedAsync(WarehouseDbContext db, CancellationToken ct = default)
    {
        var existing = await db.Recipes
            .Select(r => r.ProductId)
            .ToHashSetAsync(ct);

        var toSeed = BuildRecipes().Where(r => !existing.Contains(r.ProductId)).ToList();
        if (toSeed.Count == 0) return;

        await db.Recipes.AddRangeAsync(toSeed, ct);
        await db.SaveChangesAsync(ct);
    }

    private static List<Recipe> BuildRecipes()
    {
        var sourdoughId   = Guid.NewGuid();
        var croissantId   = Guid.NewGuid();
        var eclairId      = Guid.NewGuid();

        return
        [
            new Recipe
            {
                Id          = sourdoughId,
                ProductId   = SourdoughLoafId,
                ProductName = "Sourdough Loaf",
                Ingredients =
                [
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = sourdoughId, IngredientName = "Bread Flour", QuantityRequired = 0.5m,  Unit = "kg" },
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = sourdoughId, IngredientName = "Yeast",       QuantityRequired = 0.01m, Unit = "kg" },
                ]
            },
            new Recipe
            {
                Id          = croissantId,
                ProductId   = ButterCroissantId,
                ProductName = "Butter Croissant",
                Ingredients =
                [
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = croissantId, IngredientName = "Bread Flour", QuantityRequired = 0.3m,  Unit = "kg"     },
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = croissantId, IngredientName = "Butter",      QuantityRequired = 0.15m, Unit = "kg"     },
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = croissantId, IngredientName = "Milk",        QuantityRequired = 0.1m,  Unit = "liters" },
                ]
            },
            new Recipe
            {
                Id          = eclairId,
                ProductId   = ChocolateEclairId,
                ProductName = "Chocolate Éclair",
                Ingredients =
                [
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = eclairId, IngredientName = "Bread Flour", QuantityRequired = 0.2m,  Unit = "kg"     },
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = eclairId, IngredientName = "Butter",      QuantityRequired = 0.1m,  Unit = "kg"     },
                    new RecipeIngredient { Id = Guid.NewGuid(), RecipeId = eclairId, IngredientName = "Milk",        QuantityRequired = 0.15m, Unit = "liters" },
                ]
            },
        ];
    }
}

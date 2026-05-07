using Shared = ChipBakery.Shared;
using Warehouse.Application.DTOs;
using Warehouse.Domain.Entities;

namespace Warehouse.Application.Mapping;

public static class WarehouseMappingExtensions
{
    public static WarehouseItemDto ToDto(this WarehouseItem item) =>
        new(item.Id, item.Name, item.Quantity, item.Unit);

    public static List<WarehouseItemDto> ToDtoList(this IEnumerable<WarehouseItem> items) =>
        items.Select(i => i.ToDto()).ToList();

    public static Shared.RecipeIngredientDto ToDto(this RecipeIngredient ing) =>
        new(ing.Id, ing.IngredientName, ing.QuantityRequired, ing.Unit);

    public static Shared.RecipeDto ToDto(this Recipe recipe) =>
        new(recipe.Id, recipe.ProductId, recipe.ProductName,
            recipe.Ingredients.Select(i => i.ToDto()).ToList());

    public static List<Shared.RecipeDto> ToDtoList(this IEnumerable<Recipe> recipes) =>
        recipes.Select(r => r.ToDto()).ToList();
}

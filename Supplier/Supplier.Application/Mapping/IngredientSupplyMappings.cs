using ChipBakery.Shared;
using Supplier.Domain.Entities;

namespace Supplier.Application.Mapping;

public static class IngredientSupplyMappings
{
    public static IngredientSupplyDto ToDto(this IngredientSupply entity)
    {
        return new IngredientSupplyDto(
            entity.Id,
            entity.IngredientName,
            entity.SupplierName,
            entity.Quantity,
            entity.Price,
            entity.ScheduledDate);
    }

    public static List<IngredientSupplyDto> ToDtoList(this IEnumerable<IngredientSupply> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    public static IngredientSupply ToEntity(this CreateIngredientSupplyRequest request)
    {
        return new IngredientSupply
        {
            Id = Guid.NewGuid(),
            IngredientName = request.IngredientName,
            SupplierName = request.SupplierName,
            Quantity = request.Quantity,
            Price = request.Price,
            ScheduledDate = request.ScheduledDate
        };
    }

    public static void ApplyUpdate(this IngredientSupply entity, UpdateIngredientSupplyRequest request)
    {
        entity.IngredientName = request.IngredientName;
        entity.SupplierName = request.SupplierName;
        entity.Quantity = request.Quantity;
        entity.Price = request.Price;
        entity.ScheduledDate = request.ScheduledDate;
    }
}

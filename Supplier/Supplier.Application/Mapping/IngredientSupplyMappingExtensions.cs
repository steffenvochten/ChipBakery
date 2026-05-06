using Supplier.Application.DTOs;
using Supplier.Domain.Entities;

namespace Supplier.Application.Mapping;

public static class IngredientSupplyMappingExtensions
{
    public static IngredientSupplyDto ToDto(this IngredientSupply entity) =>
        new(
            entity.Id,
            entity.IngredientName,
            entity.SupplierName,
            entity.Quantity,
            entity.Price,
            entity.ScheduledDate);

    public static List<IngredientSupplyDto> ToDtoList(this IEnumerable<IngredientSupply> entities) =>
        entities.Select(e => e.ToDto()).ToList();
}

using Supplier.Application.DTOs;
using Supplier.Domain.Entities;

namespace Supplier.Application.Mapping;

public static class SupplierTransportMappingExtensions
{
    public static SupplierTransportDto ToDto(this SupplierTransport entity)
    {
        return new SupplierTransportDto(
            entity.Id,
            entity.IngredientName,
            entity.Quantity,
            entity.Unit,
            entity.Timestamp);
    }

    public static List<SupplierTransportDto> ToDtoList(this IEnumerable<SupplierTransport> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}

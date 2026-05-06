using Warehouse.Application.DTOs;
using Warehouse.Domain.Entities;

namespace Warehouse.Application.Mapping;

public static class WarehouseMappingExtensions
{
    public static WarehouseItemDto ToDto(this WarehouseItem item) =>
        new(item.Id, item.Name, item.Quantity, item.Unit);

    public static List<WarehouseItemDto> ToDtoList(this IEnumerable<WarehouseItem> items) =>
        items.Select(i => i.ToDto()).ToList();
}

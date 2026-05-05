using Inventory.Application.DTOs;
using Inventory.Domain.Entities;

namespace Inventory.Application.Mapping;

/// <summary>
/// Extension methods to map between domain entities and application DTOs.
/// No third-party mapping library needed — keeps the dependency graph clean.
/// </summary>
public static class InventoryMappingExtensions
{
    public static InventoryItemDto ToDto(this InventoryItem item) =>
        new(item.Id, item.Name, item.Price, item.Quantity);

    public static List<InventoryItemDto> ToDtoList(this IEnumerable<InventoryItem> items) =>
        items.Select(i => i.ToDto()).ToList();
}

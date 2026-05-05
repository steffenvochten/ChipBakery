namespace Inventory.Application.DTOs;

/// <summary>
/// Read model returned to callers for any inventory item query.
/// </summary>
public record InventoryItemDto(
    Guid Id,
    string Name,
    decimal Price,
    int Quantity);

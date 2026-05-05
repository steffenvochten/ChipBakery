namespace Inventory.Domain.Events;

/// <summary>
/// Raised when an inventory item's stock reaches zero after a deduction.
/// Consumers: Warehouse.Service (trigger restock), alerts, dashboards.
/// </summary>
public record StockDepletedEvent(
    Guid ItemId,
    string ItemName,
    DateTime OccurredAt);

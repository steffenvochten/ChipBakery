namespace Inventory.Domain.Events;

/// <summary>
/// Raised when an inventory item is permanently removed from the catalogue.
/// Consumers: Frontend cache invalidation, audit log.
/// </summary>
public record InventoryItemDeletedEvent(
    Guid ItemId,
    string ItemName,
    DateTime OccurredAt);

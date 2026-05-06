namespace Inventory.Domain.Events;

/// <summary>
/// Raised when a new inventory item is added to the catalogue.
/// Consumers: Product catalogue sync, frontend cache invalidation.
/// </summary>
public record InventoryItemCreatedEvent(
    Guid ItemId,
    string ItemName,
    decimal Price,
    int InitialQuantity,
    DateTime OccurredAt);

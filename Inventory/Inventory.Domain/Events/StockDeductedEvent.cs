namespace Inventory.Domain.Events;

/// <summary>
/// Raised when stock has been successfully deducted from an inventory item.
/// Consumers: Order.Service confirmation, analytics, audit log.
/// </summary>
public record StockDeductedEvent(
    Guid ItemId,
    string ItemName,
    int QuantityDeducted,
    int RemainingQuantity,
    DateTime OccurredAt);

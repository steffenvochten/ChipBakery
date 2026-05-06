namespace Warehouse.Domain.Events;

public record StockDeductedEvent(Guid ItemId, double QuantityDeducted, double RemainingQuantity);

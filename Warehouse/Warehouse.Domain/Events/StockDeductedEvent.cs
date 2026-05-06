namespace Warehouse.Domain.Events;

public record StockDeductedEvent(Guid ItemId, decimal QuantityDeducted, decimal RemainingQuantity);

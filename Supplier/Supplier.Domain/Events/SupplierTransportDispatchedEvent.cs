namespace Supplier.Domain.Events;

public record SupplierTransportDispatchedEvent(
    Guid Id, 
    string IngredientName, 
    decimal Quantity, 
    string Unit, 
    DateTime Timestamp);

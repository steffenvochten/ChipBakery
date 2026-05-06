namespace Supplier.Domain.Events;

public record IngredientSupplyCreatedEvent(Guid Id, string IngredientName, string SupplierName, int Quantity);

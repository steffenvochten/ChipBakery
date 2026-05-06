namespace Supplier.Domain.Events;

public record IngredientSupplyDeletedEvent(Guid Id, string IngredientName);

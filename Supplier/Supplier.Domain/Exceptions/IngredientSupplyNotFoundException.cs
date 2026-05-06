namespace Supplier.Domain.Exceptions;

public class IngredientSupplyNotFoundException(Guid id) 
    : DomainException($"Ingredient supply with ID '{id}' was not found.");

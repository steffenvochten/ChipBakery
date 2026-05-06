namespace Supplier.Application.DTOs;

public record IngredientSupplyDto(
    Guid Id,
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

public record CreateIngredientSupplyRequest(
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

public record UpdateIngredientSupplyRequest(
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

namespace Supplier.Application.DTOs;

public record SupplierTransportDto(
    Guid Id,
    string IngredientName,
    decimal Quantity,
    string Unit,
    DateTime Timestamp);

public record DispatchTransportRequest(
    string IngredientName,
    decimal Quantity,
    string Unit);

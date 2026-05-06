namespace Warehouse.Application.DTOs;

public record WarehouseItemDto(Guid Id, string Name, decimal Quantity, string Unit);

public record UpdateWarehouseItemRequest(string Name, decimal Quantity, string Unit);

public record DeductStockRequest(Guid ItemId, decimal Quantity);

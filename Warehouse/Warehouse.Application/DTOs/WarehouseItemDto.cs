namespace Warehouse.Application.DTOs;

public record WarehouseItemDto(Guid Id, string Name, double Quantity, string Unit);

public record CreateWarehouseItemRequest(string Name, double Quantity, string Unit);

public record UpdateWarehouseItemRequest(string Name, double Quantity, string Unit);

public record DeductStockRequest(Guid ItemId, double Quantity);

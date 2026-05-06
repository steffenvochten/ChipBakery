namespace Warehouse.Domain.Events;

public record WarehouseItemCreatedEvent(Guid Id, string Name, decimal InitialQuantity, string Unit);

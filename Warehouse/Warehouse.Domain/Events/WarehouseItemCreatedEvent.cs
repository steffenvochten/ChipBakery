namespace Warehouse.Domain.Events;

public record WarehouseItemCreatedEvent(Guid Id, string Name, double InitialQuantity, string Unit);

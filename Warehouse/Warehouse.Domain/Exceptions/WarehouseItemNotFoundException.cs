namespace Warehouse.Domain.Exceptions;

public class WarehouseItemNotFoundException(Guid id) 
    : DomainException($"Warehouse item with ID {id} was not found.");

namespace Warehouse.Domain.Exceptions;

public class InsufficientStockException(string name, double requested, double available, string unit) 
    : DomainException($"Insufficient stock for '{name}'. Requested: {requested}{unit}, Available: {available}{unit}.");

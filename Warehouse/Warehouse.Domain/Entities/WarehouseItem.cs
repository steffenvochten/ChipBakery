namespace Warehouse.Domain.Entities;

public class WarehouseItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty; // e.g., "kg", "liters", "units"
}

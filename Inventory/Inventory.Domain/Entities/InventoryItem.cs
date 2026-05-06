namespace Inventory.Domain.Entities;

/// <summary>
/// Represents a finished bakery product available for sale.
/// This is a pure data entity; all business logic lives in the Application layer.
/// </summary>
public class InventoryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

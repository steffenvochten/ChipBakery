namespace Supplier.Domain.Entities;

/// <summary>
/// Represents an ingredient supply scheduled to be received from a supplier.
/// </summary>
public class IngredientSupply
{
    public Guid Id { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime ScheduledDate { get; set; }
}

namespace Supplier.Domain.Entities;

/// <summary>
/// Represents a transport of ingredients from a supplier.
/// </summary>
public class SupplierTransport
{
    public Guid Id { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

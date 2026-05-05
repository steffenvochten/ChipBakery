using ChipBakery.Shared;

namespace Order.Domain.Entities;

/// <summary>
/// Represents a customer order placed at the bakery.
/// This is a pure data entity; all business logic lives in the Application layer.
/// </summary>
public class BakeryOrder
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    /// <summary>Total cost = unit price × quantity, captured at order time.</summary>
    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
}

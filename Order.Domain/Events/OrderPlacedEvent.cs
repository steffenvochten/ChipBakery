namespace Order.Domain.Events;

/// <summary>
/// Published when a new order is successfully placed and persisted.
/// Will be routed to Production.Service via RabbitMQ when the real event bus is wired up.
/// </summary>
/// <param name="OrderId">Unique identifier of the placed order.</param>
/// <param name="CustomerName">Name of the customer who placed the order.</param>
/// <param name="ProductId">ID of the ordered product (references Inventory.Service).</param>
/// <param name="Quantity">Number of units ordered.</param>
/// <param name="TotalPrice">Total cost captured at order time (unit price × quantity).</param>
/// <param name="PlacedAt">UTC timestamp when the order was placed.</param>
public record OrderPlacedEvent(
    Guid OrderId,
    string CustomerName,
    Guid ProductId,
    int Quantity,
    decimal TotalPrice,
    DateTime PlacedAt);

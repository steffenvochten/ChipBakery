namespace Order.Domain.Exceptions;

/// <summary>
/// Thrown when an order with the specified ID does not exist in the database.
/// Mapped to HTTP 404 Not Found by GlobalExceptionHandler.
/// </summary>
public class OrderNotFoundException : DomainException
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"Order '{orderId}' was not found.")
    {
        OrderId = orderId;
    }
}

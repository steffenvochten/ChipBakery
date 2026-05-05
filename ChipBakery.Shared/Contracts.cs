namespace ChipBakery.Shared;

public record ProductItem(Guid Id, string Name, decimal Price, int AvailableQuantity);

public record OrderRequest(Guid ProductId, int Quantity, string CustomerName);

public record OrderResponse(bool Success, string Message, Guid? OrderId = null);

/// <summary>
/// Abstraction for publishing domain events to an external message broker.
/// Currently implemented by MockEventPublisher (structured logging) in each service.
/// Replace with a RabbitMQ/MassTransit implementation per service when ready.
/// Shared here to avoid duplication across service Domain projects.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event. The event type drives routing/exchange binding
    /// in the real broker implementation.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}

/// <summary>
/// Lifecycle states for a bakery order.
/// Stored as a string in the database for readability.
/// </summary>
public enum OrderStatus
{
    Placed,
    Processing,
    Completed,
    Cancelled
}

// ─── Web Frontend DTOs ────────────────────────────────────────────────────
// These match the read models returned by the Services to simplify deserialization.

public record OrderItem(Guid Id, string CustomerName, Guid ProductId, int Quantity, decimal TotalPrice, OrderStatus Status, DateTime OrderDate);

public record InventoryItem(Guid Id, string Name, decimal Price, int Quantity);

public record CreateInventoryRequest(string Name, decimal Price, int Quantity);

public record UpdateInventoryRequest(string Name, decimal Price, int Quantity);

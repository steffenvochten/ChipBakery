namespace Order.Domain.Events;

/// <summary>
/// Published when a customer cancels an existing order.
/// Currently only orders with status <c>Placed</c> can be cancelled.
/// </summary>
/// <param name="OrderId">Unique identifier of the cancelled order.</param>
/// <param name="CancelledAt">UTC timestamp when the cancellation was processed.</param>
public record OrderCancelledEvent(
    Guid OrderId,
    DateTime CancelledAt);

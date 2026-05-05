using Order.Application.DTOs;

namespace Order.Application.Interfaces;

/// <summary>
/// Application service contract for all order operations.
/// Implemented by <see cref="Order.Application.Services.OrderService"/>.
/// </summary>
public interface IOrderService
{
    /// <summary>Returns all orders, most recent first.</summary>
    Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single order by ID.
    /// Throws <see cref="Order.Domain.Exceptions.OrderNotFoundException"/> if not found.
    /// </summary>
    Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Places a new order by synchronously validating and deducting stock from Inventory.Service,
    /// then persisting the order and publishing an <see cref="Order.Domain.Events.OrderPlacedEvent"/>.
    /// </summary>
    Task<OrderDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Cancels an existing order. Only orders with status <c>Placed</c> can be cancelled.
    /// Throws <see cref="Order.Domain.Exceptions.OrderNotFoundException"/> if not found.
    /// Throws <see cref="System.InvalidOperationException"/> if the order is not in a cancellable state.
    /// </summary>
    Task CancelOrderAsync(Guid id, CancellationToken ct = default);
}

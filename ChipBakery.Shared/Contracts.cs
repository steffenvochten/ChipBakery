namespace ChipBakery.Shared;

public record ProductItem(Guid Id, string Name, decimal Price, int AvailableQuantity);

public record OrderRequest(Guid ProductId, int Quantity, string CustomerName, string CustomerId);

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

// ─── Warehouse DTOs ───────────────────────────────────────────────────────

public record WarehouseItem(Guid Id, string Name, decimal Quantity, string Unit);

public record RecipeCheckRequest(Guid ProductId, int Quantity);

public record RecipeCheckResponse(bool Available, string? Message = null);

public class CreateWarehouseItemRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}

// ─── Production DTOs ──────────────────────────────────────────────────────

public record BakingJob(Guid Id, Guid ProductId, decimal Quantity, string Status, DateTime? StartTime, DateTime? EndTime);

// ─── Loyalty DTOs ─────────────────────────────────────────────────────────

public record CustomerLoyalty(Guid CustomerId, int TotalPoints, string Tier, List<LoyaltyTransaction> Transactions);

public record LoyaltyTransaction(Guid Id, int Points, DateTime Date, string Description);

// ─── Supplier DTOs ────────────────────────────────────────────────────────

public record SupplierTransportDto(Guid Id, string IngredientName, decimal Quantity, string Unit, DateTime Timestamp);

public record DispatchTransportRequest(string IngredientName, decimal Quantity, string Unit);

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
/// <param name="CustomerId">Unique identifier of the customer.</param>
public record OrderPlacedEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerId,
    Guid ProductId,
    int Quantity,
    decimal TotalPrice,
    DateTime PlacedAt);

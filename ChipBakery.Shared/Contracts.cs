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

public record RecipeIngredientDto(Guid Id, string IngredientName, decimal QuantityRequired, string Unit);
public record RecipeDto(Guid Id, Guid ProductId, string ProductName, List<RecipeIngredientDto> Ingredients);
public record CreateRecipeIngredientRequest(string IngredientName, decimal QuantityRequired, string Unit);
public record CreateRecipeRequest(Guid ProductId, string ProductName, List<CreateRecipeIngredientRequest> Ingredients);

public record RecipeCheckRequest(Guid ProductId, int Quantity);

public record RecipeCheckResponse(bool Available, string? Message = null);

/// <summary>
/// Atomic "check + deduct" request used by Production.Service when a baking job starts.
/// If all required ingredients are present in sufficient quantity, the warehouse deducts them
/// in a single transaction; otherwise nothing is deducted and the response carries the shortage detail.
/// </summary>
public record ConsumeRecipeRequest(Guid ProductId, int Quantity);

public record ConsumeRecipeResponse(
    bool Consumed,
    string? ShortageIngredientName = null,
    decimal? ShortageQuantityNeeded = null,
    decimal? ShortageQuantityAvailable = null,
    string? ShortageUnit = null,
    string? Message = null);

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

public record RestockRequest(string IngredientName, decimal Quantity, string Unit);

public record IngredientSupplyDto(
    Guid Id,
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

public record CreateIngredientSupplyRequest(
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

public record UpdateIngredientSupplyRequest(
    string IngredientName,
    string SupplierName,
    int Quantity,
    decimal Price,
    DateTime ScheduledDate);

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

/// <summary>
/// Published when a customer cancels an existing order.
/// </summary>
public record OrderCancelledEvent(
    Guid OrderId,
    DateTime CancelledAt);

/// <summary>
/// Published by Production.Service when a baking job has finished.
/// Consumed by Loyalty.Service to award bonus points.
/// </summary>
public record JobCompletedEvent(
    Guid JobId,
    Guid ProductId,
    decimal Quantity,
    DateTime CompletedAt);

/// <summary>
/// Published by Supplier.Service when a transport is dispatched toward the warehouse.
/// Consumed by Warehouse.Service to increment raw-material stock on arrival.
/// </summary>
public record SupplierTransportDispatchedEvent(
    Guid Id,
    string IngredientName,
    decimal Quantity,
    string Unit,
    DateTime Timestamp);

/// <summary>
/// Published by Production.Service when a baking job cannot start because at least one
/// ingredient is below the recipe requirement. The job is held in "AwaitingIngredients"
/// status and re-checked on each tick of the BakingProgressWorker; agents listening for
/// this event can react by triggering a supplier restock.
/// </summary>
public record IngredientShortageEvent(
    Guid JobId,
    Guid ProductId,
    decimal Quantity,
    string IngredientName,
    decimal QuantityNeeded,
    decimal QuantityAvailable,
    string Unit,
    DateTime DetectedAt);

/// <summary>
/// Canonical baking-job status strings stored on <c>BakingJob.Status</c>.
/// Centralised here so producers and agents agree on the exact spelling.
/// </summary>
public static class BakingJobStatus
{
    public const string Scheduled = "Scheduled";
    public const string AwaitingIngredients = "AwaitingIngredients";
    public const string Baking = "Baking";
    public const string Completed = "Completed";
}

// ─── Inventory restock ────────────────────────────────────────────────────────

/// <summary>Request body for POST /api/inventory/{id}/restock.</summary>
public record AddInventoryStockRequest(int Quantity);

// ─── Agent Activity ───────────────────────────────────────────────────────────

/// <summary>
/// A single observable action taken by an autonomous agent.
/// Broadcast via SignalR from Agents.Service to the Web frontend in real time.
/// </summary>
public record AgentActivity(
    string AgentName,
    string AgentType,
    string Action,
    string Narration,
    DateTime Timestamp);

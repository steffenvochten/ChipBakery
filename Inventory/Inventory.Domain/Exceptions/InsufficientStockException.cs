namespace Inventory.Domain.Exceptions;

/// <summary>
/// Thrown when a stock deduction is requested but available quantity is insufficient.
/// Maps to HTTP 409 Conflict via the exception handler middleware.
/// </summary>
public class InsufficientStockException : DomainException
{
    public Guid ItemId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(Guid itemId, int requested, int available)
        : base($"Insufficient stock for item '{itemId}': requested {requested}, available {available}.")
    {
        ItemId = itemId;
        Requested = requested;
        Available = available;
    }
}

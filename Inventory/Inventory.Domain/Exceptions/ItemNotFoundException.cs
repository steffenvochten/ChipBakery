namespace Inventory.Domain.Exceptions;

/// <summary>
/// Thrown when an inventory item with the specified ID does not exist.
/// Maps to HTTP 404 Not Found via the exception handler middleware.
/// </summary>
public class ItemNotFoundException : DomainException
{
    public Guid ItemId { get; }

    public ItemNotFoundException(Guid itemId)
        : base($"Inventory item with ID '{itemId}' was not found.")
    {
        ItemId = itemId;
    }
}

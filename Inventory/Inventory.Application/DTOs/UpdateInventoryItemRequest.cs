namespace Inventory.Application.DTOs;

/// <summary>
/// Input model for updating an existing inventory item.
/// Validated by <see cref="Inventory.Application.Validators.UpdateInventoryItemValidator"/>.
/// </summary>
public record UpdateInventoryItemRequest(
    string Name,
    decimal Price,
    int Quantity);

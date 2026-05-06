namespace Inventory.Application.DTOs;

/// <summary>
/// Input model for creating a new inventory item.
/// Validated by <see cref="Inventory.Application.Validators.CreateInventoryItemValidator"/>.
/// </summary>
public record CreateInventoryItemRequest(
    string Name,
    decimal Price,
    int Quantity);

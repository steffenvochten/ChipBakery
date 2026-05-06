using Inventory.Application.DTOs;

namespace Inventory.Application.Interfaces;

/// <summary>
/// Application service contract for all inventory operations.
/// Implemented by <see cref="Inventory.Application.Services.InventoryService"/>.
/// </summary>
public interface IInventoryService
{
    /// <summary>Returns all inventory items regardless of stock level.</summary>
    Task<List<InventoryItemDto>> GetAllItemsAsync(CancellationToken ct = default);

    /// <summary>Returns only items with Quantity > 0 (available for ordering).</summary>
    Task<List<InventoryItemDto>> GetAvailableItemsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single item by ID.
    /// Throws <see cref="Inventory.Domain.Exceptions.ItemNotFoundException"/> if not found.
    /// </summary>
    Task<InventoryItemDto> GetItemByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new inventory item.
    /// Publishes <see cref="Inventory.Domain.Events.InventoryItemCreatedEvent"/>.
    /// </summary>
    Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing inventory item.
    /// Throws <see cref="Inventory.Domain.Exceptions.ItemNotFoundException"/> if not found.
    /// </summary>
    Task<InventoryItemDto> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes an inventory item.
    /// Throws <see cref="Inventory.Domain.Exceptions.ItemNotFoundException"/> if not found.
    /// Publishes <see cref="Inventory.Domain.Events.InventoryItemDeletedEvent"/>.
    /// </summary>
    Task DeleteItemAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deducts stock from an inventory item after validating sufficient quantity.
    /// Throws <see cref="Inventory.Domain.Exceptions.ItemNotFoundException"/> if item not found.
    /// Throws <see cref="Inventory.Domain.Exceptions.InsufficientStockException"/> if not enough stock.
    /// Publishes <see cref="Inventory.Domain.Events.StockDeductedEvent"/> (and optionally <see cref="Inventory.Domain.Events.StockDepletedEvent"/>).
    /// </summary>
    Task DeductStockAsync(DeductStockRequest request, CancellationToken ct = default);
}

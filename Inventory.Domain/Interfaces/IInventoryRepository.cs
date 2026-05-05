using Inventory.Domain.Entities;

namespace Inventory.Domain.Interfaces;

/// <summary>
/// Defines the data access contract for inventory items.
/// EF change-tracking is preserved: Update and Delete do not fetch new copies —
/// they operate on the already-tracked entity passed in. Call SaveChangesAsync to commit.
/// </summary>
public interface IInventoryRepository
{
    /// <summary>Returns a single inventory item by ID, or null if not found.</summary>
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all inventory items regardless of stock level.</summary>
    Task<List<InventoryItem>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns only items with Quantity > 0.</summary>
    Task<List<InventoryItem>> GetAvailableAsync(CancellationToken ct = default);

    /// <summary>Adds a new item to the change tracker (not yet persisted).</summary>
    Task AddAsync(InventoryItem item, CancellationToken ct = default);

    /// <summary>Marks an already-tracked item as modified (not yet persisted).</summary>
    void Update(InventoryItem item);

    /// <summary>Marks an already-tracked item for deletion (not yet persisted).</summary>
    void Delete(InventoryItem item);

    /// <summary>Commits all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

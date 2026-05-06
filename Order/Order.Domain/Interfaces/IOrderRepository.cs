using Order.Domain.Entities;

namespace Order.Domain.Interfaces;

/// <summary>
/// Defines the data access contract for bakery orders.
/// EF change-tracking is preserved: Update and Delete do not fetch new copies —
/// they operate on the already-tracked entity passed in. Call SaveChangesAsync to commit.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Returns a single order by ID, or null if not found.</summary>
    Task<BakeryOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all orders regardless of status, most recent first.</summary>
    Task<List<BakeryOrder>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Adds a new order to the change tracker (not yet persisted).</summary>
    Task AddAsync(BakeryOrder order, CancellationToken ct = default);

    /// <summary>Marks an already-tracked order as modified (not yet persisted).</summary>
    void Update(BakeryOrder order);

    /// <summary>Marks an already-tracked order for deletion (not yet persisted).</summary>
    void Delete(BakeryOrder order);

    /// <summary>Commits all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

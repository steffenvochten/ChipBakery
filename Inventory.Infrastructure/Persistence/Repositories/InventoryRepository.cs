using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IInventoryRepository"/>.
/// Deliberately thin — no business logic here, just data access.
/// EF change tracking is leveraged so Update/Delete don't require re-fetching entities.
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _context;

    public InventoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Items.FindAsync([id], ct);

    /// <inheritdoc/>
    public async Task<List<InventoryItem>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Items.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<List<InventoryItem>> GetAvailableAsync(CancellationToken ct = default) =>
        await _context.Items
            .AsNoTracking()
            .Where(i => i.Quantity > 0)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(InventoryItem item, CancellationToken ct = default) =>
        await _context.Items.AddAsync(item, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// The entity must already be tracked by this context (i.e. loaded via GetByIdAsync in the same scope).
    /// EF will generate an UPDATE statement covering only the changed columns.
    /// </remarks>
    public void Update(InventoryItem item) =>
        _context.Items.Update(item);

    /// <inheritdoc/>
    /// <remarks>
    /// The entity must already be tracked by this context. EF generates a DELETE statement on SaveChanges.
    /// </remarks>
    public void Delete(InventoryItem item) =>
        _context.Items.Remove(item);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

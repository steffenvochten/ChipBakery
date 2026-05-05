using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Domain.Interfaces;

namespace Order.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOrderRepository"/>.
/// Deliberately thin — no business logic here, just data access.
/// EF change tracking is leveraged so Update/Delete don't require re-fetching entities.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<BakeryOrder?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Orders.FindAsync([id], ct);

    /// <inheritdoc/>
    /// <remarks>Returns orders newest-first by OrderDate for a better default UX.</remarks>
    public async Task<List<BakeryOrder>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task AddAsync(BakeryOrder order, CancellationToken ct = default) =>
        await _context.Orders.AddAsync(order, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// The entity must already be tracked by this context (i.e. loaded via GetByIdAsync in the same scope).
    /// EF will generate an UPDATE statement covering only the changed columns.
    /// </remarks>
    public void Update(BakeryOrder order) =>
        _context.Orders.Update(order);

    /// <inheritdoc/>
    /// <remarks>
    /// The entity must already be tracked by this context. EF generates a DELETE statement on SaveChanges.
    /// </remarks>
    public void Delete(BakeryOrder order) =>
        _context.Orders.Remove(order);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

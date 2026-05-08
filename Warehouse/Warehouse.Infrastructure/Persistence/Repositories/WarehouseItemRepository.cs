using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Infrastructure.Persistence.Repositories;

public class WarehouseItemRepository(WarehouseDbContext context) : IWarehouseRepository
{
    public async Task<WarehouseItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Items.FindAsync([id], ct);

    public async Task<WarehouseItem?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await context.Items.FirstOrDefaultAsync(i => i.Name == name, ct);

    public async Task<List<WarehouseItem>> GetByNamesAsync(List<string> names, CancellationToken ct = default) =>
        await context.Items
            .Where(i => names.Contains(i.Name))
            .ToListAsync(ct);

    public async Task<List<WarehouseItem>> GetAllAsync(CancellationToken ct = default) =>
        await context.Items.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(WarehouseItem item, CancellationToken ct = default) =>
        await context.Items.AddAsync(item, ct);

    public void Update(WarehouseItem item) =>
        context.Items.Update(item);

    public void Delete(WarehouseItem item) =>
        context.Items.Remove(item);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}

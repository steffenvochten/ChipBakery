using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;
using Supplier.Domain.Interfaces;

namespace Supplier.Infrastructure.Persistence.Repositories;

public class IngredientSupplyRepository(SupplierDbContext context) : IIngredientSupplyRepository
{
    public async Task<IngredientSupply?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.IngredientSupplies.FindAsync([id], ct);
    }

    public async Task<List<IngredientSupply>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.IngredientSupplies
            .OrderByDescending(x => x.ScheduledDate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(IngredientSupply item, CancellationToken ct = default)
    {
        await context.IngredientSupplies.AddAsync(item, ct);
    }

    public void Update(IngredientSupply item)
    {
        context.IngredientSupplies.Update(item);
    }

    public void Remove(IngredientSupply item)
    {
        context.IngredientSupplies.Remove(item);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}

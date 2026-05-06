using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;
using Supplier.Domain.Interfaces;
using Supplier.Infrastructure.Persistence;

namespace Supplier.Infrastructure.Persistence.Repositories;

public class IngredientSupplyRepository(SupplierDbContext context) : IIngredientSupplyRepository
{
    private readonly SupplierDbContext _context = context;

    public async Task<IngredientSupply?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.IngredientSupplies.FindAsync([id], ct);

    public async Task<List<IngredientSupply>> GetAllAsync(CancellationToken ct = default) =>
        await _context.IngredientSupplies.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(IngredientSupply item, CancellationToken ct = default) =>
        await _context.IngredientSupplies.AddAsync(item, ct);

    public void Update(IngredientSupply item) =>
        _context.IngredientSupplies.Update(item);

    public void Delete(IngredientSupply item) =>
        _context.IngredientSupplies.Remove(item);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}

using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Infrastructure.Persistence.Repositories;

public class RecipeRepository(WarehouseDbContext context) : IRecipeRepository
{
    public async Task<Recipe?> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        await context.Recipes
            .Include(r => r.Ingredients)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ProductId == productId, ct);

    public async Task AddAsync(Recipe recipe, CancellationToken ct = default) =>
        await context.Recipes.AddAsync(recipe, ct);

    public async Task<List<Recipe>> GetAllAsync(CancellationToken ct = default) =>
        await context.Recipes
            .Include(r => r.Ingredients)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}

using Warehouse.Domain.Entities;

namespace Warehouse.Domain.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(Recipe recipe, CancellationToken ct = default);
    Task<List<Recipe>> GetAllAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

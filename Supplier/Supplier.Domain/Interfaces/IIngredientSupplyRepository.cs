using Supplier.Domain.Entities;

namespace Supplier.Domain.Interfaces;

public interface IIngredientSupplyRepository
{
    Task<IngredientSupply?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<IngredientSupply>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(IngredientSupply item, CancellationToken ct = default);
    void Update(IngredientSupply item);
    void Delete(IngredientSupply item);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

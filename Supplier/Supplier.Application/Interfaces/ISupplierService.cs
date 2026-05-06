using Supplier.Application.DTOs;

namespace Supplier.Application.Interfaces;

public interface ISupplierService
{
    Task<IngredientSupplyDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<IngredientSupplyDto>> GetAllAsync(CancellationToken ct = default);
    Task<IngredientSupplyDto> CreateAsync(CreateIngredientSupplyRequest request, CancellationToken ct = default);
    Task<IngredientSupplyDto> UpdateAsync(Guid id, UpdateIngredientSupplyRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

using ChipBakery.Shared;

namespace Supplier.Application.Interfaces;

public interface ISupplierService
{
    Task<Supplier.Application.DTOs.SupplierTransportDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Supplier.Application.DTOs.SupplierTransportDto>> GetAllAsync(CancellationToken ct = default);
    Task<Supplier.Application.DTOs.SupplierTransportDto> DispatchTransportAsync(Supplier.Application.DTOs.DispatchTransportRequest request, CancellationToken ct = default);

    // Ingredient supply CRUD
    Task<IngredientSupplyDto> CreateIngredientAsync(CreateIngredientSupplyRequest request, CancellationToken ct = default);
    Task<IngredientSupplyDto> UpdateIngredientAsync(Guid id, UpdateIngredientSupplyRequest request, CancellationToken ct = default);
    Task DeleteIngredientAsync(Guid id, CancellationToken ct = default);
    Task<List<IngredientSupplyDto>> ListIngredientsAsync(CancellationToken ct = default);
    Task<IngredientSupplyDto?> GetIngredientByIdAsync(Guid id, CancellationToken ct = default);

    // Restock — creates a transport, persists it, publishes SupplierTransportDispatchedEvent
    Task<ChipBakery.Shared.SupplierTransportDto> RestockAsync(RestockRequest request, CancellationToken ct = default);
}

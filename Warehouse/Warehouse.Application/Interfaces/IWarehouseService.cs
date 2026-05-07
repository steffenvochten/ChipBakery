using ChipBakery.Shared;
using Warehouse.Application.DTOs;

namespace Warehouse.Application.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseItemDto>> GetAllItemsAsync(CancellationToken ct = default);
    Task<WarehouseItemDto> GetItemByIdAsync(Guid id, CancellationToken ct = default);
    Task<WarehouseItemDto> CreateItemAsync(CreateWarehouseItemRequest request, CancellationToken ct = default);
    Task<WarehouseItemDto> UpdateItemAsync(Guid id, UpdateWarehouseItemRequest request, CancellationToken ct = default);
    Task DeleteItemAsync(Guid id, CancellationToken ct = default);
    Task DeductStockAsync(DeductStockRequest request, CancellationToken ct = default);
    Task<RecipeCheckResponse> CheckRecipeAsync(RecipeCheckRequest request, CancellationToken ct = default);
    Task<ConsumeRecipeResponse> ConsumeRecipeAsync(ConsumeRecipeRequest request, CancellationToken ct = default);
}

using ChipBakery.Shared;

namespace Order.Application.Interfaces;

/// <summary>
/// Synchronous HTTP client for interacting with Warehouse.Service.
/// </summary>
public interface IWarehouseClient
{
    /// <summary>
    /// Verifies if the required ingredients for a product are available in the warehouse.
    /// </summary>
    Task<RecipeCheckResponse> CheckRecipeAsync(Guid productId, int quantity, CancellationToken ct = default);
}

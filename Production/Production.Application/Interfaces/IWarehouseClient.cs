using ChipBakery.Shared;

namespace Production.Application.Interfaces;

/// <summary>
/// Synchronous HTTP client used by Production.Service to talk to Warehouse.Service.
/// Production needs to atomically check + deduct ingredients when starting a baking job;
/// if any ingredient is short, the job is held in <see cref="BakingJobStatus.AwaitingIngredients"/>.
/// </summary>
public interface IWarehouseClient
{
    Task<ConsumeRecipeResponse> ConsumeRecipeAsync(Guid productId, int quantity, CancellationToken ct = default);
}

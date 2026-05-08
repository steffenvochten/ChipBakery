using ChipBakery.Shared;

namespace Catalog.Application.Interfaces;

public interface ICatalogService
{
    Task<RecipeOrchestrationResponse> OrchestrateRecipeCreationAsync(CreateRecipeOrchestrationRequest request, CancellationToken ct);
}

using ChipBakery.Shared;
using Catalog.Application.Interfaces;

namespace Catalog.Service;

public static class CatalogEndpoints
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/catalog")
            .WithTags("Catalog");

        group.MapPost("/recipes", async (CreateRecipeOrchestrationRequest request, ICatalogService svc, CancellationToken ct) =>
        {
            var result = await svc.OrchestrateRecipeCreationAsync(request, ct);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("OrchestrateRecipeCreation")
        .WithSummary("Orchestrates creation of ingredients, products, recipes and agents across the ecosystem.");

        return app;
    }
}

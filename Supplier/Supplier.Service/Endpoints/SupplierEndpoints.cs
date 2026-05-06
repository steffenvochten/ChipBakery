using ChipBakery.Shared;
using Supplier.Application.DTOs;
using Supplier.Application.Interfaces;

namespace Supplier.Service.Endpoints;

public static class SupplierEndpoints
{
    public static WebApplication MapSupplierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/supplier")
            .WithTags("Supplier");

        group.MapGet("/", async (ISupplierService svc, CancellationToken ct) =>
        {
            var transports = await svc.GetAllAsync(ct);
            return Results.Ok(transports);
        })
        .WithName("GetAllTransports");

        group.MapPost("/dispatch", async (Supplier.Application.DTOs.DispatchTransportRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            await svc.DispatchTransportAsync(request, ct);
            return Results.Accepted();
        })
        .WithName("DispatchTransport");

        // ─── Ingredient supply CRUD ─────────────────────────────────────────
        group.MapGet("/ingredients", async (ISupplierService svc, CancellationToken ct) =>
        {
            var items = await svc.ListIngredientsAsync(ct);
            return Results.Ok(items);
        })
        .WithName("ListIngredients");

        group.MapGet("/ingredients/{id:guid}", async (Guid id, ISupplierService svc, CancellationToken ct) =>
        {
            var item = await svc.GetIngredientByIdAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("GetIngredientById");

        group.MapPost("/ingredients", async (CreateIngredientSupplyRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateIngredientAsync(request, ct);
            return Results.CreatedAtRoute("GetIngredientById", new { id = created.Id }, created);
        })
        .WithName("CreateIngredient");

        group.MapPut("/ingredients/{id:guid}", async (Guid id, UpdateIngredientSupplyRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            var updated = await svc.UpdateIngredientAsync(id, request, ct);
            return Results.Ok(updated);
        })
        .WithName("UpdateIngredient");

        group.MapDelete("/ingredients/{id:guid}", async (Guid id, ISupplierService svc, CancellationToken ct) =>
        {
            await svc.DeleteIngredientAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteIngredient");

        group.MapPost("/restock", async (RestockRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            var transport = await svc.RestockAsync(request, ct);
            return Results.Ok(transport);
        })
        .WithName("Restock");

        return app;
    }
}

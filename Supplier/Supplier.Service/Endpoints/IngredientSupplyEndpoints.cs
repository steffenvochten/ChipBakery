using Supplier.Application.DTOs;
using Supplier.Application.Interfaces;

namespace Supplier.Service.Endpoints;

public static class IngredientSupplyEndpoints
{
    public static WebApplication MapIngredientSupplyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/supplier")
            .WithTags("Supplier");

        group.MapGet("/", async (ISupplierService svc, CancellationToken ct) =>
        {
            var supplies = await svc.GetAllAsync(ct);
            return Results.Ok(supplies);
        })
        .WithName("GetAllIngredientSupplies");

        group.MapGet("/{id:guid}", async (Guid id, ISupplierService svc, CancellationToken ct) =>
        {
            var supply = await svc.GetByIdAsync(id, ct);
            return Results.Ok(supply);
        })
        .WithName("GetIngredientSupplyById");

        group.MapPost("/", async (CreateIngredientSupplyRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            var supply = await svc.CreateAsync(request, ct);
            return Results.CreatedAtRoute("GetIngredientSupplyById", new { id = supply.Id }, supply);
        })
        .WithName("CreateIngredientSupply");

        group.MapPut("/{id:guid}", async (Guid id, UpdateIngredientSupplyRequest request, ISupplierService svc, CancellationToken ct) =>
        {
            var supply = await svc.UpdateAsync(id, request, ct);
            return Results.Ok(supply);
        })
        .WithName("UpdateIngredientSupply");

        group.MapDelete("/{id:guid}", async (Guid id, ISupplierService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteIngredientSupply");

        return app;
    }
}

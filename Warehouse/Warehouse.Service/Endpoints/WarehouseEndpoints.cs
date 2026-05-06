using Warehouse.Application.DTOs;
using Warehouse.Application.Interfaces;

namespace Warehouse.Service.Endpoints;

public static class WarehouseEndpoints
{
    public static WebApplication MapWarehouseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/warehouse")
            .WithTags("Warehouse");

        group.MapGet("/", async (IWarehouseService svc, CancellationToken ct) =>
        {
            var items = await svc.GetAllItemsAsync(ct);
            return Results.Ok(items);
        })
        .WithName("GetAllWarehouseItems");

        group.MapGet("/{id:guid}", async (Guid id, IWarehouseService svc, CancellationToken ct) =>
        {
            var item = await svc.GetItemByIdAsync(id, ct);
            return Results.Ok(item);
        })
        .WithName("GetWarehouseItemById");

        group.MapPost("/", async (CreateWarehouseItemRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            var item = await svc.CreateItemAsync(request, ct);
            return Results.CreatedAtRoute("GetWarehouseItemById", new { id = item.Id }, item);
        })
        .WithName("CreateWarehouseItem");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWarehouseItemRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            var item = await svc.UpdateItemAsync(id, request, ct);
            return Results.Ok(item);
        })
        .WithName("UpdateWarehouseItem");

        group.MapDelete("/{id:guid}", async (Guid id, IWarehouseService svc, CancellationToken ct) =>
        {
            await svc.DeleteItemAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteWarehouseItem");

        group.MapPost("/deduct", async (DeductStockRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            await svc.DeductStockAsync(request, ct);
            return Results.Ok();
        })
        .WithName("DeductWarehouseStock");

        return app;
    }
}

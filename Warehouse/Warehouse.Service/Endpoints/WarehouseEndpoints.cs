using ChipBakery.Shared;
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

        group.MapPost("/check-recipe", async (RecipeCheckRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            var result = await svc.CheckRecipeAsync(request, ct);
            return Results.Ok(result);
        })
        .WithName("CheckRecipe");

        group.MapPost("/consume-recipe", async (ConsumeRecipeRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            var result = await svc.ConsumeRecipeAsync(request, ct);
            return Results.Ok(result);
        })
        .WithName("ConsumeRecipe");

        group.MapGet("/recipes", async (IWarehouseService svc, CancellationToken ct) =>
        {
            var recipes = await svc.GetAllRecipesAsync(ct);
            return Results.Ok(recipes);
        })
        .WithName("GetAllRecipes");

        group.MapPost("/recipes", async (CreateRecipeRequest request, IWarehouseService svc, CancellationToken ct) =>
        {
            var recipe = await svc.UpsertRecipeAsync(request, ct);
            return Results.Ok(recipe);
        })
        .WithName("UpsertRecipe");

        group.MapDelete("/recipes/{productId:guid}", async (Guid productId, IWarehouseService svc, CancellationToken ct) =>
        {
            await svc.DeleteRecipeAsync(productId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteRecipe");

        return app;
    }
}

using ChipBakery.Shared;
using FluentValidation;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;

namespace Inventory.Service.Endpoints;

/// <summary>
/// Defines all HTTP endpoints for the Inventory API.
/// This class is the only place that references ChipBakery.Shared types — they are
/// mapped to/from internal Application DTOs here at the API boundary.
/// </summary>
public static class InventoryEndpoints
{
    public static WebApplication MapInventoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory");

        // ─── Queries ──────────────────────────────────────────────────────────────

        group.MapGet("/", async (IInventoryService svc, CancellationToken ct) =>
        {
            var items = await svc.GetAllItemsAsync(ct);
            return Results.Ok(items);
        })
        .WithName("GetAllInventoryItems")
        .WithSummary("Returns all inventory items regardless of stock level.");

        group.MapGet("/available", async (IInventoryService svc, CancellationToken ct) =>
        {
            var items = await svc.GetAvailableItemsAsync(ct);

            // Map to ChipBakery.Shared.ProductItem for backward compatibility
            // with Order.Service and the Blazor frontend.
            var productItems = items.Select(i => new ProductItem(i.Id, i.Name, i.Price, i.Quantity));
            return Results.Ok(productItems);
        })
        .WithName("GetAvailableInventoryItems")
        .WithSummary("Returns only items with stock > 0. Used by the Blazor storefront.");

        group.MapGet("/{id:guid}", async (Guid id, IInventoryService svc, CancellationToken ct) =>
        {
            var item = await svc.GetItemByIdAsync(id, ct);
            return Results.Ok(item);
        })
        .WithName("GetInventoryItemById")
        .WithSummary("Returns a single inventory item by ID.");

        // ─── Commands ─────────────────────────────────────────────────────────────

        group.MapPost("/", async (CreateInventoryItemRequest request, IInventoryService svc, CancellationToken ct) =>
        {
            var item = await svc.CreateItemAsync(request, ct);
            return Results.CreatedAtRoute("GetInventoryItemById", new { id = item.Id }, item);
        })
        .WithName("CreateInventoryItem")
        .WithSummary("Creates a new inventory item.");

        group.MapPut("/{id:guid}", async (Guid id, UpdateInventoryItemRequest request, IInventoryService svc, CancellationToken ct) =>
        {
            var item = await svc.UpdateItemAsync(id, request, ct);
            return Results.Ok(item);
        })
        .WithName("UpdateInventoryItem")
        .WithSummary("Updates an existing inventory item's name, price, and quantity.");

        group.MapDelete("/{id:guid}", async (Guid id, IInventoryService svc, CancellationToken ct) =>
        {
            await svc.DeleteItemAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteInventoryItem")
        .WithSummary("Permanently removes an inventory item from the catalogue.");

        // ─── Internal endpoints ───────────────────────────────────────────────────

        group.MapPost("/{id:guid}/restock", async (Guid id, AddInventoryStockRequest request, IInventoryService svc, CancellationToken ct) =>
        {
            var item = await svc.RestockAsync(id, request.Quantity, ct);
            return Results.Ok(item);
        })
        .WithName("RestockInventoryItem")
        .WithSummary("Adds finished-goods stock after a baking job completes. Called by the Baker agent.");

        group.MapPost("/deduct", async (OrderRequest request, IInventoryService svc, CancellationToken ct) =>
        {
            // Map ChipBakery.Shared.OrderRequest → internal DeductStockRequest.
            // This is the only place that knows about both types — the application
            // layer has no dependency on ChipBakery.Shared.
            var deductRequest = new DeductStockRequest(request.ProductId, request.Quantity);
            var result = await svc.DeductStockAsync(deductRequest, ct);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("DeductInventoryStock")
        .WithSummary("Deducts stock from an item. Called synchronously by Order.Service before accepting an order.");

        return app;
    }
}

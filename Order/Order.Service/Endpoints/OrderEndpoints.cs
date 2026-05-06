using ChipBakery.Shared;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Service.Endpoints;

/// <summary>
/// Defines all HTTP endpoints for the Order API.
/// This class is the ONLY place that references ChipBakery.Shared types — they are
/// mapped to/from internal Application DTOs here at the API boundary.
/// </summary>
public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // ─── Queries ──────────────────────────────────────────────────────────────

        group.MapGet("/", async (IOrderService svc, CancellationToken ct) =>
        {
            var orders = await svc.GetAllOrdersAsync(ct);
            return Results.Ok(orders);
        })
        .WithName("GetAllOrders")
        .WithSummary("Returns all orders, most recent first.");

        group.MapGet("/{id:guid}", async (Guid id, IOrderService svc, CancellationToken ct) =>
        {
            var order = await svc.GetOrderByIdAsync(id, ct);
            return Results.Ok(order);
        })
        .WithName("GetOrderById")
        .WithSummary("Returns a single order by ID.");

        // ─── Commands ─────────────────────────────────────────────────────────────

        group.MapPost("/", async (OrderRequest request, IOrderService svc, CancellationToken ct) =>
        {
            // Map ChipBakery.Shared.OrderRequest → internal PlaceOrderRequest.
            // This is the only place that knows about both types — the Application
            // layer has no dependency on ChipBakery.Shared.
            var placeRequest = new PlaceOrderRequest(request.ProductId, request.Quantity, request.CustomerName, request.CustomerId);
            var order = await svc.PlaceOrderAsync(placeRequest, ct);

            // Return ChipBakery.Shared.OrderResponse for backward compatibility
            // with the Blazor frontend (ChipBakery.Web/Services/BakeryApiClient.cs).
            return Results.CreatedAtRoute(
                "GetOrderById",
                new { id = order.Id },
                new OrderResponse(true, "Order placed successfully!", order.Id));
        })
        .WithName("PlaceOrder")
        .WithSummary("Places a new order. Synchronously validates and deducts stock from Inventory.Service.");

        group.MapPut("/{id:guid}/cancel", async (Guid id, IOrderService svc, CancellationToken ct) =>
        {
            await svc.CancelOrderAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("CancelOrder")
        .WithSummary("Cancels an order. Only orders with status 'Placed' can be cancelled.");

        return app;
    }
}

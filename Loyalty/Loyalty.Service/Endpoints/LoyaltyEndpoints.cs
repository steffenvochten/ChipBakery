using Loyalty.Application.DTOs;
using Loyalty.Application.Interfaces;

namespace Loyalty.Service.Endpoints;

public static class LoyaltyEndpoints
{
    public static WebApplication MapLoyaltyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loyalty")
            .WithTags("Loyalty");

        group.MapGet("/{customerId:guid}", async (Guid customerId, ILoyaltyService svc, CancellationToken ct) =>
        {
            var loyalty = await svc.GetByCustomerIdAsync(customerId, ct);
            return loyalty != null ? Results.Ok(loyalty) : Results.NotFound();
        })
        .WithName("GetCustomerLoyalty");

        group.MapPost("/award", async (AwardPointsRequest request, ILoyaltyService svc, CancellationToken ct) =>
        {
            await svc.AwardPointsAsync(request.CustomerId, request.Points, request.Description, ct);
            return Results.Accepted();
        })
        .WithName("AwardLoyaltyPoints");

        return app;
    }
}

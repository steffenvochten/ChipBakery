using Loyalty.Application.DTOs;
using Loyalty.Application.Interfaces;

namespace Loyalty.Service.Endpoints;

public static class LoyaltyEndpoints
{
    public static WebApplication MapLoyaltyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loyalty")
            .WithTags("Loyalty");

        group.MapGet("/", async (ILoyaltyService svc, CancellationToken ct) =>
        {
            var members = await svc.GetAllAsync(ct);
            return Results.Ok(members);
        })
        .WithName("GetAllLoyaltyMembers");

        group.MapGet("/{id:guid}", async (Guid id, ILoyaltyService svc, CancellationToken ct) =>
        {
            var member = await svc.GetByIdAsync(id, ct);
            return Results.Ok(member);
        })
        .WithName("GetLoyaltyMemberById");

        group.MapPost("/", async (CreateLoyaltyMemberRequest request, ILoyaltyService svc, CancellationToken ct) =>
        {
            var member = await svc.CreateAsync(request, ct);
            return Results.CreatedAtRoute("GetLoyaltyMemberById", new { id = member.Id }, member);
        })
        .WithName("CreateLoyaltyMember");

        group.MapPost("/add-points", async (AddPointsRequest request, ILoyaltyService svc, CancellationToken ct) =>
        {
            var member = await svc.AddPointsAsync(request, ct);
            return Results.Ok(member);
        })
        .WithName("AddLoyaltyPoints");

        group.MapPost("/deduct-points", async (DeductPointsRequest request, ILoyaltyService svc, CancellationToken ct) =>
        {
            var member = await svc.DeductPointsAsync(request, ct);
            return Results.Ok(member);
        })
        .WithName("DeductLoyaltyPoints");

        return app;
    }
}

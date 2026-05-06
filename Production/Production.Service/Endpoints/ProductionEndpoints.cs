using Production.Application.DTOs;
using Production.Application.Interfaces;

namespace Production.Service.Endpoints;

public static class ProductionEndpoints
{
    public static WebApplication MapProductionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/production")
            .WithTags("Production");

        group.MapGet("/", async (IBakingScheduleService svc, CancellationToken ct) =>
        {
            var schedules = await svc.GetAllAsync(ct);
            return Results.Ok(schedules);
        })
        .WithName("GetAllBakingSchedules");

        group.MapGet("/{id:guid}", async (Guid id, IBakingScheduleService svc, CancellationToken ct) =>
        {
            var schedule = await svc.GetByIdAsync(id, ct);
            return Results.Ok(schedule);
        })
        .WithName("GetBakingScheduleById");

        group.MapPost("/", async (CreateBakingScheduleRequest request, IBakingScheduleService svc, CancellationToken ct) =>
        {
            var schedule = await svc.CreateAsync(request, ct);
            return Results.CreatedAtRoute("GetBakingScheduleById", new { id = schedule.Id }, schedule);
        })
        .WithName("CreateBakingSchedule");

        group.MapPost("/{id:guid}/start", async (Guid id, IBakingScheduleService svc, CancellationToken ct) =>
        {
            await svc.StartBakingAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("StartBaking");

        group.MapPost("/{id:guid}/complete", async (Guid id, IBakingScheduleService svc, CancellationToken ct) =>
        {
            await svc.CompleteBakingAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("CompleteBaking");

        return app;
    }
}

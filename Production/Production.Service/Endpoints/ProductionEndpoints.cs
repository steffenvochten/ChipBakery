using Production.Application.DTOs;
using Production.Application.Interfaces;

namespace Production.Service.Endpoints;

public static class ProductionEndpoints
{
    public static WebApplication MapProductionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/production")
            .WithTags("Production");

        group.MapGet("/", async (IBakingService svc, CancellationToken ct) =>
        {
            var jobs = await svc.GetAllJobsAsync(ct);
            return Results.Ok(jobs);
        })
        .WithName("GetAllBakingJobs");

        group.MapPost("/schedule", async (ScheduleBakingJobRequest request, IBakingService svc, CancellationToken ct) =>
        {
            var job = await svc.ScheduleJobAsync(request, ct);
            return Results.Ok(job);
        })
        .WithName("ScheduleBakingJob");

        group.MapPost("/{id:guid}/start", async (Guid id, IBakingService svc, CancellationToken ct) =>
        {
            await svc.StartJobAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("StartBakingJob");

        group.MapPost("/{id:guid}/complete", async (Guid id, IBakingService svc, CancellationToken ct) =>
        {
            await svc.CompleteJobAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("CompleteBakingJob");

        return app;
    }
}

using ChipBakery.Shared;
using Production.Application.Interfaces;

namespace Production.Service.Workers;

/// <summary>
/// Drives the baking-job lifecycle on a periodic tick:
/// <list type="bullet">
///   <item>Scheduled jobs are attempted: ingredients consumed → Baking, or held as AwaitingIngredients with an event.</item>
///   <item>AwaitingIngredients jobs are retried each tick — if a supplier delivery has restocked the warehouse, they advance.</item>
///   <item>Baking jobs complete after <see cref="BakingDuration"/> elapsed since their start time.</item>
/// </list>
/// Sized for the demo: short tick (3s) and short baking time (8s) so the loop is visible in real time.
/// </summary>
public class BakingProgressWorker(
    IServiceProvider serviceProvider,
    ILogger<BakingProgressWorker> logger) : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan BakingDuration = TimeSpan.FromSeconds(8);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "BakingProgressWorker started (tick={TickSeconds}s, bake={BakeSeconds}s)",
            TickInterval.TotalSeconds, BakingDuration.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "BakingProgressWorker tick failed");
            }

            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var baking = scope.ServiceProvider.GetRequiredService<IBakingService>();

        var scheduled = await baking.GetJobsByStatusAsync(BakingJobStatus.Scheduled, ct);
        foreach (var job in scheduled)
        {
            await baking.TryStartJobAsync(job.Id, ct);
        }

        var awaiting = await baking.GetJobsByStatusAsync(BakingJobStatus.AwaitingIngredients, ct);
        foreach (var job in awaiting)
        {
            await baking.TryStartJobAsync(job.Id, ct);
        }

        var bakingJobs = await baking.GetJobsByStatusAsync(BakingJobStatus.Baking, ct);
        var now = DateTime.UtcNow;
        foreach (var job in bakingJobs)
        {
            if (job.StartTime is { } start && now - start >= BakingDuration)
            {
                await baking.CompleteJobAsync(job.Id, ct);
            }
        }
    }
}

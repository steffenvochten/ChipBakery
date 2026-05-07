using System.Net.Http.Json;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Workers;

/// <summary>
/// Baker agent that monitors the production queue and narrates every job
/// status transition. Uses the LLM to generate flavourful first-person narration;
/// falls back to canned messages when Ollama is unavailable.
/// </summary>
public class BakerAgent(
    IServiceProvider services,
    IAgentBrain brain,
    IHubContext<AgentActivityHub> hub,
    ILogger<BakerAgent> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(12);

    private const string SystemPrompt =
        "You are the Baker at Chip Bakery — skilled, a little floury, and passionate about your craft. " +
        "You narrate your baking work in short, vivid first-person sentences. " +
        "Keep it under 20 words and sound like a real baker, not a robot.";

    // Tracks jobId → last observed status
    private readonly Dictionary<Guid, string> _jobStates = new();

    // Product name cache; refreshed every N ticks
    private Dictionary<Guid, string> _productNames = new();
    private int _tickCount = 0;
    private const int ProductCacheRefreshEvery = 8;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BakerAgent started");
        await DelayAsync(TimeSpan.FromSeconds(8 + Random.Shared.Next(2, 5)), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "BakerAgent tick threw"); }

            await DelayAsync(Interval, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        _tickCount++;

        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // ── 1. Refresh product name cache periodically ────────────────────
        if (_tickCount == 1 || _tickCount % ProductCacheRefreshEvery == 0)
            await RefreshProductNamesAsync(factory, ct);

        // ── 2. Fetch all production jobs ───────────────────────────────────
        var production = factory.CreateClient("Production");
        List<JobSnapshot>? jobs;
        try { jobs = await production.GetFromJsonAsync<List<JobSnapshot>>("/api/production", ct); }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Baker could not reach production service");
            return;
        }

        if (jobs == null || jobs.Count == 0) return;

        // ── 3. Detect transitions and narrate ─────────────────────────────
        int narrated = 0;
        foreach (var job in jobs)
        {
            if (narrated >= 2) break; // Cap LLM calls per tick

            _jobStates.TryGetValue(job.Id, out var prevStatus);

            if (prevStatus == job.Status) continue; // No change

            _jobStates[job.Id] = job.Status;

            if (prevStatus == null)
            {
                // Newly appeared job — only narrate if it's already active (not just Scheduled)
                if (job.Status == BakingJobStatus.Baking || job.Status == BakingJobStatus.AwaitingIngredients)
                    await NarrateTransitionAsync(job, null, job.Status, ct);
                continue;
            }

            await NarrateTransitionAsync(job, prevStatus, job.Status, ct);
            narrated++;
        }

        // Remove jobs no longer returned by the API (completed and cleaned up)
        var currentIds = jobs.Select(j => j.Id).ToHashSet();
        foreach (var stale in _jobStates.Keys.Except(currentIds).ToList())
            _jobStates.Remove(stale);
    }

    private async Task NarrateTransitionAsync(
        JobSnapshot job, string? from, string to, CancellationToken ct)
    {
        var qty         = (int)job.Quantity;
        var productName = _productNames.TryGetValue(job.ProductId, out var n) ? n : "product";

        var (action, fallback) = (from, to) switch
        {
            (_, BakingJobStatus.Baking) =>
                ("started-baking",
                 $"Firing up the oven — baking {qty} {productName}!"),

            (BakingJobStatus.Baking, BakingJobStatus.Completed) =>
                ("batch-completed",
                 $"Fresh batch done — {qty} {productName} out of the oven."),

            (_, BakingJobStatus.Completed) =>
                ("batch-completed",
                 $"Wrapped up a {qty}-unit {productName} batch."),

            (_, BakingJobStatus.AwaitingIngredients) =>
                ("awaiting-ingredients",
                 $"Can't start {qty} {productName} — short on ingredients. Waiting on a delivery."),

            (BakingJobStatus.AwaitingIngredients, _) =>
                ("ingredients-arrived",
                 $"Ingredients just arrived — resuming the {productName} batch!"),

            _ => ("status-update", $"Job for {qty} {productName} is now {to}.")
        };

        // Ask LLM for colourful narration
        var context = $"""
            Situation: {(from == null ? $"Job just appeared as {to}" : $"Job changed from {from} to {to}")}
            Product: {productName}, Quantity: {qty}

            Write one short sentence (under 20 words) in first person as the Baker.
            Reply with exactly: NARRATION: <one sentence>
            """;

        var llmResponse = await brain.ThinkAsync(SystemPrompt, context, ct);
        var narration   = TryParseNarration(llmResponse) ?? fallback;

        await BroadcastAsync(action, narration, ct);
    }

    private async Task RefreshProductNamesAsync(IHttpClientFactory factory, CancellationToken ct)
    {
        try
        {
            var inventory = factory.CreateClient("Inventory");
            var products  = await inventory.GetFromJsonAsync<List<ProductItem>>("/api/inventory/available", ct);
            if (products != null)
                _productNames = products.ToDictionary(p => p.Id, p => p.Name);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Baker could not refresh product names");
        }
    }

    private static string? TryParseNarration(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return null;
        var line = response.Split('\n')
            .FirstOrDefault(l => l.TrimStart().StartsWith("NARRATION:", StringComparison.OrdinalIgnoreCase));
        return line != null ? line.Split(':', 2)[1].Trim() : null;
    }

    private async Task BroadcastAsync(string action, string narration, CancellationToken ct)
    {
        var activity = new AgentActivity("Baker", "Baker", action, narration, DateTime.UtcNow);
        await hub.Clients.All.SendAsync("ReceiveActivity", activity, ct);
        logger.LogInformation("[Baker] {Action}: {Narration}", action, narration);
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (TaskCanceledException) { }
    }

    private sealed record JobSnapshot(
        Guid Id, Guid ProductId, decimal Quantity, string Status,
        DateTime? StartTime, DateTime? EndTime);
}

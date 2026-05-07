using System.Net.Http.Json;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Workers;

/// <summary>
/// Baker agent that:
///  1. Monitors all inventory items and proactively schedules production runs
///     when any product stock falls below the reorder threshold.
///  2. Narrates every production job status transition via LLM.
///  3. Calls POST /api/inventory/{id}/restock when a job completes so that
///     finished goods actually appear on the shelf for clients to order.
/// </summary>
public class BakerAgent(
    IServiceProvider services,
    IAgentBrain brain,
    AgentSettings settings,
    IHubContext<AgentActivityHub> hub,
    ILogger<BakerAgent> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(12);

    private const int LowStockThreshold = 10;  // units below which a restock batch is triggered
    private const int BatchSize          = 20;  // units baked per production run

    private const string SystemPrompt =
        "You are the Baker at Chip Bakery — skilled, a little floury, and passionate about your craft. " +
        "You narrate your baking work in short, vivid first-person sentences. " +
        "Keep it under 20 words and sound like a real baker, not a robot.";

    // jobId → last observed status
    private readonly Dictionary<Guid, string> _jobStates = new();

    // jobs for which we have already posted an inventory restock (avoid double-crediting)
    private readonly HashSet<Guid> _restockedJobs = new();

    // productId → name (populated from full inventory list each tick)
    private Dictionary<Guid, string> _productNames = new();

    // Jobs that were already Completed when the agent first started — don't retroactively restock those
    private bool _startupScanDone = false;
    private readonly DateTime _startedAt = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BakerAgent started");
        await AgentDelay.SmartAsync(TimeSpan.FromSeconds(8 + Random.Shared.Next(2, 5)), settings, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "BakerAgent tick threw"); }

            await AgentDelay.SmartAsync(Interval, settings, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var inventoryClient = factory.CreateClient("Inventory");
        var productionClient = factory.CreateClient("Production");

        // ── 1. Fetch ALL inventory items (includes zero-stock) ────────────
        List<InventorySnapshot> allItems;
        try { allItems = await inventoryClient.GetFromJsonAsync<List<InventorySnapshot>>("/api/inventory", ct) ?? []; }
        catch (Exception ex) { logger.LogDebug(ex, "Baker could not reach inventory service"); return; }

        // Rebuild product name cache from the full list
        _productNames = allItems.ToDictionary(p => p.Id, p => p.Name);

        // ── 2. Fetch all production jobs ───────────────────────────────────
        List<JobSnapshot> jobs;
        try { jobs = await productionClient.GetFromJsonAsync<List<JobSnapshot>>("/api/production", ct) ?? []; }
        catch (Exception ex) { logger.LogDebug(ex, "Baker could not reach production service"); return; }

        // Product IDs that already have a non-completed production job in flight
        var inProduction = jobs
            .Where(j => j.Status != BakingJobStatus.Completed)
            .Select(j => j.ProductId)
            .ToHashSet();

        // ── 3. Schedule production for low-stock items ─────────────────────
        foreach (var item in allItems)
        {
            if (item.Quantity >= LowStockThreshold) continue;
            if (inProduction.Contains(item.Id)) continue; // already queued

            var body = new { ProductId = item.Id, Quantity = (decimal)BatchSize };
            var resp = await productionClient.PostAsJsonAsync("/api/production/schedule", body, ct);

            if (resp.IsSuccessStatusCode)
            {
                inProduction.Add(item.Id); // prevent re-scheduling within same tick
                await BroadcastAsync("scheduled-production",
                    $"Only {item.Quantity} {item.Name} left — kicking off a batch of {BatchSize}.", ct);
            }
        }

        // On first tick, mark all already-completed jobs so we don't retroactively restock them
        if (!_startupScanDone)
        {
            _startupScanDone = true;
            foreach (var j in jobs.Where(j => j.Status == BakingJobStatus.Completed))
                _restockedJobs.Add(j.Id);
        }

        // ── 4. Handle job transitions: narrate + restock on completion ─────
        int narrated = 0;
        foreach (var job in jobs)
        {
            _jobStates.TryGetValue(job.Id, out var prevStatus);
            if (prevStatus == job.Status) continue;
            _jobStates[job.Id] = job.Status;

            // Restock inventory the first time we observe a job as Completed.
            // prevStatus may be null when a job races through Scheduled→Baking→Completed
            // within a single 12s poll interval — we still need to restock in that case.
            // _restockedJobs guards against double-crediting, and startup scan above
            // prevents retroactively restocking jobs that completed before we started.
            if (job.Status == BakingJobStatus.Completed && _restockedJobs.Add(job.Id))
            {
                var addQty = (int)Math.Ceiling(job.Quantity);
                var restockResp = await inventoryClient.PostAsJsonAsync(
                    $"/api/inventory/{job.ProductId}/restock",
                    new AddInventoryStockRequest(addQty), ct);

                if (!restockResp.IsSuccessStatusCode)
                    logger.LogWarning("Inventory restock failed for product {ProductId}: HTTP {Status}",
                        job.ProductId, (int)restockResp.StatusCode);
            }

            // Narrate the transition (cap at 2 LLM calls per tick)
            if (narrated < 2)
            {
                await NarrateTransitionAsync(job, prevStatus, job.Status, ct);
                narrated++;
            }
        }

        // Prune jobs that have been removed from the production API response
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
                 $"Fired up the oven — baking {qty} {productName}!"),

            (BakingJobStatus.Baking, BakingJobStatus.Completed) =>
                ("batch-completed",
                 $"Fresh batch done — {qty} {productName} on the shelf."),

            (_, BakingJobStatus.Completed) =>
                ("batch-completed",
                 $"Wrapped up {qty} {productName} and restocked the shelf."),

            (_, BakingJobStatus.AwaitingIngredients) =>
                ("awaiting-ingredients",
                 $"Can't start {qty} {productName} — short on ingredients, waiting on delivery."),

            (BakingJobStatus.AwaitingIngredients, _) =>
                ("ingredients-arrived",
                 $"Ingredients arrived — resuming the {productName} batch!"),

            _ => ("status-update", $"Job for {qty} {productName} is now {to}.")
        };

        var context = $"""
            Situation: {(from == null ? $"Job appeared as {to}" : $"Job changed from {from} to {to}")}
            Product: {productName}, Quantity: {qty}

            Write one short sentence (under 20 words) in first person as the Baker.
            Reply with exactly: NARRATION: <one sentence>
            """;

        var llmResponse = await brain.ThinkAsync(SystemPrompt, context, ct);
        var narration   = TryParseNarration(llmResponse) ?? fallback;

        await BroadcastAsync(action, narration, ct);
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


    private sealed record InventorySnapshot(Guid Id, string Name, decimal Price, int Quantity);

    private sealed record JobSnapshot(
        Guid Id, Guid ProductId, decimal Quantity, string Status,
        DateTime? StartTime, DateTime? EndTime);
}

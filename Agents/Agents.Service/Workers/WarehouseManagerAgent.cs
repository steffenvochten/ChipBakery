using System.Net.Http.Json;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Workers;

/// <summary>
/// Single warehouse manager agent that monitors overall ingredient stock,
/// dispatches emergency restocks for critically depleted items, and retries
/// production jobs that were stalled on AwaitingIngredients once deliveries arrive.
/// </summary>
public class WarehouseManagerAgent(
    IServiceProvider services,
    IAgentBrain brain,
    IHubContext<AgentActivityHub> hub,
    ILogger<WarehouseManagerAgent> logger) : BackgroundService
{
    private const decimal LowThreshold      = 20m;
    private const decimal CriticalThreshold = 8m;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(28);

    private int _quietTicks = 0;

    private const string SystemPrompt =
        "You are the Warehouse Manager at Chip Bakery. You oversee all ingredient inventory " +
        "and coordinate with suppliers to keep the bakery well-stocked. " +
        "You are experienced, pragmatic, and decisive — you keep your remarks brief.";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WarehouseManagerAgent started");
        await DelayAsync(TimeSpan.FromSeconds(18 + Random.Shared.Next(2, 6)), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "WarehouseManagerAgent tick threw"); }

            await DelayAsync(Interval, stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // ── 1. Fetch warehouse stock ───────────────────────────────────────
        var warehouse = factory.CreateClient("Warehouse");
        var items = await warehouse.GetFromJsonAsync<List<WarehouseItem>>("/api/warehouse", ct) ?? [];

        var lowItems      = items.Where(i => i.Quantity < LowThreshold).ToList();
        var criticalItems = items.Where(i => i.Quantity < CriticalThreshold).ToList();

        // ── 2. Retry stalled production jobs if stock improved ────────────
        var production = factory.CreateClient("Production");
        await RetryAwaitingIngredientsJobsAsync(production, ct);

        // ── 3. All healthy — narrate sparingly ────────────────────────────
        if (lowItems.Count == 0)
        {
            _quietTicks++;
            if (_quietTicks % 4 == 0)
                await BroadcastAsync("monitoring-stock",
                    $"Ran the numbers — all {items.Count} warehouse items above threshold. We're in good shape.", ct);
            return;
        }

        _quietTicks = 0;

        // ── 4. Ask LLM to narrate the situation ───────────────────────────
        var lowLines = string.Join("\n", lowItems.Select(i =>
            $"  - {i.Name}: {i.Quantity:F1} {i.Unit}{(i.Quantity < CriticalThreshold ? " [CRITICAL]" : " (low)")}"));

        var context = $"""
            Warehouse stock report — items below reorder threshold:
            {lowLines}

            Briefly describe (ONE sentence, first person) what you are doing about this.
            Reply with exactly: NARRATION: <one sentence>
            """;

        var llmResponse = await brain.ThinkAsync(SystemPrompt, context, ct);
        var narration   = TryParseNarration(llmResponse)
                          ?? $"Flagging {lowItems.Count} low-stock item(s) — coordinating with suppliers now.";

        await BroadcastAsync("stock-alert", narration, ct);

        // ── 5. Emergency restock for critical items ────────────────────────
        var supplier = factory.CreateClient("Supplier");
        foreach (var item in criticalItems.Take(2))
        {
            var qty  = 40m;
            var req  = new RestockRequest(item.Name, qty, item.Unit);
            var resp = await supplier.PostAsJsonAsync("/api/supplier/restock", req, ct);

            if (resp.IsSuccessStatusCode)
                await BroadcastAsync("emergency-restock",
                    $"Emergency order placed: {qty} {item.Unit} of {item.Name} — critically low at {item.Quantity:F1}.", ct);
            else
                await BroadcastAsync("restock-failed",
                    $"Emergency restock request for {item.Name} failed (HTTP {(int)resp.StatusCode}).", ct);
        }
    }

    private async Task RetryAwaitingIngredientsJobsAsync(HttpClient production, CancellationToken ct)
    {
        List<StalledJob>? stalledJobs;
        try
        {
            stalledJobs = await production.GetFromJsonAsync<List<StalledJob>>("/api/production", ct);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not reach production service for stalled-job check");
            return;
        }

        if (stalledJobs == null) return;

        var awaitingJobs = stalledJobs
            .Where(j => string.Equals(j.Status, BakingJobStatus.AwaitingIngredients, StringComparison.OrdinalIgnoreCase))
            .Take(3)
            .ToList();

        if (awaitingJobs.Count == 0) return;

        await BroadcastAsync("checking-stalled-jobs",
            $"Found {awaitingJobs.Count} job(s) waiting on ingredients — checking if deliveries arrived.", ct);

        foreach (var job in awaitingJobs)
        {
            var resp = await production.PostAsync($"/api/production/{job.Id}/start", null, ct);
            if (resp.IsSuccessStatusCode)
                await BroadcastAsync("restarted-job",
                    $"Job {job.Id.ToString()[..8]}… restarted — ingredients were restocked in time.", ct);
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
        var activity = new AgentActivity("Warehouse Mgr", "Warehouse", action, narration, DateTime.UtcNow);
        await hub.Clients.All.SendAsync("ReceiveActivity", activity, ct);
        logger.LogInformation("[Warehouse Mgr] {Action}: {Narration}", action, narration);
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); }
        catch (TaskCanceledException) { }
    }

    // Minimal projection of the production job — we only need Id and Status.
    private sealed record StalledJob(Guid Id, Guid ProductId, decimal Quantity, string Status,
        DateTime? StartTime, DateTime? EndTime);
}

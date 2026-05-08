using System.Collections.Concurrent;
using System.Net.Http.Json;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Agents.Service.Workers;

/// <summary>
/// Runs one autonomous supplier agent per entry in Suppliers config (appsettings.json).
/// Each supplier specialises in a set of ingredient keywords and polls the warehouse every
/// N seconds. When any specialty ingredient is critically low the LLM decides how much to
/// dispatch; the agent then calls POST /api/supplier/restock.
/// Falls back to rule-based defaults when Ollama is unavailable.
/// Add or modify suppliers by editing appsettings.json — no code change required.
/// </summary>
public class SupplierAgentWorker(
    IServiceProvider services,
    IAgentBrain brain,
    AgentSettings settings,
    ISupplierManager supplierManager,
    IOptionsMonitor<SupplierAgentOptions> optionsMonitor,
    IHubContext<AgentActivityHub> hub,
    ILogger<SupplierAgentWorker> logger) : BackgroundService
{
    private readonly ConcurrentDictionary<string, Task> _activeLoops = new();
    private CancellationTokenSource? _stoppingCts;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var ct = _stoppingCts.Token;

        // 1. Seed from config
        var initialPersonas = optionsMonitor.CurrentValue.Suppliers
            .Select(c => new SupplierPersona(
                c.Name,
                c.SystemPrompt,
                c.Keywords,
                c.LowThreshold,
                c.DefaultRestockQty,
                TimeSpan.FromSeconds(c.IntervalSeconds)));

        foreach (var p in initialPersonas)
        {
            supplierManager.RegisterSupplier(p);
        }

        // 2. Start existing ones
        foreach (var p in supplierManager.GetActiveSuppliers())
        {
            StartLoop(p, ct);
        }

        // 3. Listen for new ones
        supplierManager.OnSupplierRegistered += (p) => StartLoop(p, ct);

        logger.LogInformation("SupplierAgentWorker started");
        
        // Wait until cancellation
        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    private void StartLoop(SupplierPersona persona, CancellationToken ct)
    {
        if (_activeLoops.TryAdd(persona.Name, null!))
        {
            var task = RunLoopAsync(persona, startDelay: _activeLoops.Count * 7 + 12, ct);
            _activeLoops[persona.Name] = task;
            logger.LogInformation("Started loop for new supplier: {Name}", persona.Name);
        }
    }

    private async Task RunLoopAsync(SupplierPersona persona, int startDelay, CancellationToken ct)
    {
        await AgentDelay.SmartAsync(TimeSpan.FromSeconds(startDelay + Random.Shared.Next(2, 6)), settings, ct);

        while (!ct.IsCancellationRequested)
        {
            try { await TickAsync(persona, ct); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "{Agent} tick threw", persona.Name); }

            await AgentDelay.SmartAsync(persona.Interval, settings, ct);
        }
    }

    private async Task TickAsync(SupplierPersona persona, CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // ── 1. Fetch warehouse stock ───────────────────────────────────────
        var warehouse = factory.CreateClient("Warehouse");
        var allItems = await warehouse.GetFromJsonAsync<List<WarehouseItem>>("/api/warehouse", ct) ?? [];

        var specialty = allItems
            .Where(i => persona.Keywords.Any(kw =>
                i.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (specialty.Count == 0)
        {
            await BroadcastAsync(persona, "monitoring-stock",
                "No specialty items in the warehouse catalog yet — checking back later.", ct);
            return;
        }

        var lowItems = specialty.Where(i => i.Quantity < persona.LowThreshold).ToList();

        if (lowItems.Count == 0)
        {
            await BroadcastAsync(persona, "monitoring-stock",
                $"All specialty stock levels look healthy. No delivery needed right now.", ct);
            return;
        }

        // ── 2. Ask LLM whether and how much to restock ────────────────────
        var lowLines = string.Join("\n",
            lowItems.Select(i => $"  - {i.Name}: {i.Quantity:F1} {i.Unit} (LOW)"));

        var context = $"""
            Current warehouse stock for your specialty ingredients:
            {lowLines}

            Should you dispatch a delivery? Reply with EXACTLY these lines and nothing else:
            SHOULD_RESTOCK: yes
            INGREDIENT: <exact ingredient name from the list above>
            QUANTITY: <number>
            UNIT: <unit from above>
            NARRATION: <one sentence explaining your decision in first person>
            """;

        var llmResponse = await brain.ThinkAsync(persona.SystemPrompt, context, ct);

        if (!TryParseDecision(llmResponse, lowItems, out var ingredient, out var qty, out var unit, out var narration))
        {
            // Fallback: restock the item with lowest stock.
            var target = lowItems.OrderBy(i => i.Quantity).First();
            ingredient = target.Name;
            qty = persona.DefaultRestockQty;
            unit = target.Unit;
            narration = $"Dispatching {qty} {unit} of {ingredient} to top up critically low stock.";
        }

        // ── 3. Dispatch the restock ────────────────────────────────────────
        var supplier = factory.CreateClient("Supplier");
        var req = new RestockRequest(ingredient, qty, unit);
        var resp = await supplier.PostAsJsonAsync("/api/supplier/restock", req, ct);

        if (resp.IsSuccessStatusCode)
            await BroadcastAsync(persona, "dispatched-delivery", narration, ct);
        else
            await BroadcastAsync(persona, "dispatch-failed",
                $"Dispatch failed for {ingredient}: HTTP {(int)resp.StatusCode}", ct);
    }

    private static bool TryParseDecision(
        string response,
        List<WarehouseItem> candidates,
        out string ingredient, out decimal qty, out string unit, out string narration)
    {
        ingredient = ""; qty = 0; unit = ""; narration = "";
        if (string.IsNullOrWhiteSpace(response)) return false;

        var lines = response.Split('\n');

        var restockLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith("SHOULD_RESTOCK:", StringComparison.OrdinalIgnoreCase));
        if (restockLine == null) return false;
        var restockVal = restockLine.Split(':', 2)[1].Trim();
        if (!restockVal.Equals("yes", StringComparison.OrdinalIgnoreCase)) return false;

        var ingLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith("INGREDIENT:", StringComparison.OrdinalIgnoreCase));
        if (ingLine == null) return false;
        ingredient = ingLine.Split(':', 2)[1].Trim();
        var ing = ingredient;
        if (!candidates.Any(c => c.Name.Equals(ing, StringComparison.OrdinalIgnoreCase)))
            return false;

        var qtyLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith("QUANTITY:", StringComparison.OrdinalIgnoreCase));
        if (qtyLine == null || !decimal.TryParse(qtyLine.Split(':', 2)[1].Trim(), out qty) || qty <= 0)
            return false;

        var unitLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith("UNIT:", StringComparison.OrdinalIgnoreCase));
        unit = unitLine != null ? unitLine.Split(':', 2)[1].Trim() : "";

        var narLine = lines.FirstOrDefault(l =>
            l.TrimStart().StartsWith("NARRATION:", StringComparison.OrdinalIgnoreCase));
        narration = narLine != null ? narLine.Split(':', 2)[1].Trim() : $"Dispatching {qty} {unit} of {ingredient}.";

        return true;
    }

    private async Task BroadcastAsync(SupplierPersona persona, string action, string narration, CancellationToken ct)
    {
        var activity = new AgentActivity(persona.Name, "Supplier", action, narration, DateTime.UtcNow);
        await hub.Clients.All.SendAsync("ReceiveActivity", activity, ct);
        logger.LogInformation("[{Agent}] {Action}: {Narration}", persona.Name, action, narration);
    }

}

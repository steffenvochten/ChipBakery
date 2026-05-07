using System.Net.Http.Json;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Workers;

/// <summary>
/// Runs three autonomous client agents concurrently.
/// Each agent uses the LLM to decide what to buy based on its personality,
/// then places a real order via the Order.Service HTTP API.
/// Falls back to rule-based defaults when Ollama is unavailable.
/// </summary>
public class ClientAgentWorker(
    IServiceProvider services,
    IAgentBrain brain,
    AgentSettings settings,
    IHubContext<AgentActivityHub> hub,
    ILogger<ClientAgentWorker> logger) : BackgroundService
{
    private sealed record Persona(
        string Name,
        string CustomerId,
        string SystemPrompt,
        int DefaultMinQty,
        int DefaultMaxQty,
        TimeSpan Interval);

    private static readonly Persona[] Personas =
    [
        new(
            Name: "Sarah",
            CustomerId: "agent-client-sarah",
            SystemPrompt:
                "You are Sarah, an office worker who visits ChipBakery a few times a week. " +
                "You buy treats for your team of 6. You love cookies and croissants. " +
                "You're budget-conscious but treat yourself occasionally. " +
                "Keep your narration warm and conversational (one short sentence).",
            DefaultMinQty: 4, DefaultMaxQty: 8,
            Interval: TimeSpan.FromSeconds(22)),

        new(
            Name: "Marco",
            CustomerId: "agent-client-marco",
            SystemPrompt:
                "You are Marco, owner of Marco's Café nearby. You buy wholesale for your café. " +
                "You always buy in bulk (15–40 items) and focus on value and consistent quality. " +
                "You often mention your café or your morning rush in your narration (one short sentence).",
            DefaultMinQty: 15, DefaultMaxQty: 30,
            Interval: TimeSpan.FromSeconds(48)),

        new(
            Name: "Lin",
            CustomerId: "agent-client-lin",
            SystemPrompt:
                "You are Lin, exploring ChipBakery for the first time. You're curious and adventurous. " +
                "You tend to overbuy slightly because everything looks amazing. " +
                "Your narration is excited and uses exclamation marks occasionally (one short sentence).",
            DefaultMinQty: 3, DefaultMaxQty: 10,
            Interval: TimeSpan.FromSeconds(33)),
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ClientAgentWorker started ({Count} agents)", Personas.Length);
        var tasks = Personas.Select((p, i) => RunLoopAsync(p, startDelay: i * 5, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task RunLoopAsync(Persona persona, int startDelay, CancellationToken ct)
    {
        // Stagger initial start so all three agents don't fire at the same moment.
        await AgentDelay.SmartAsync(TimeSpan.FromSeconds(startDelay + Random.Shared.Next(2, 8)), settings, ct);

        while (!ct.IsCancellationRequested)
        {
            try { await TickAsync(persona, ct); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "{Agent} tick threw", persona.Name); }

            await AgentDelay.SmartAsync(persona.Interval, settings, ct);
        }
    }

    private async Task TickAsync(Persona persona, CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // ── 1. Fetch available products ────────────────────────────────────
        var inv = factory.CreateClient("Inventory");
        var products = await inv.GetFromJsonAsync<List<ProductItem>>("/api/inventory/available", ct) ?? [];

        if (products.Count == 0)
        {
            await BroadcastAsync(persona, "browsing",
                "The shelves look empty today — I'll check back soon.", ct);
            return;
        }

        // ── 2. Ask LLM what to buy ─────────────────────────────────────────
        var maxQty = persona.Name == "Marco" ? 40 : 12;
        var productLines = string.Join("\n",
            products.Take(8).Select(p =>
                $"  - {p.Name}: ${p.Price:F2} each, {p.AvailableQuantity} in stock  ID={p.Id}"));

        var context = $"""
            Available products at ChipBakery right now:
            {productLines}

            Choose ONE product. Reply with EXACTLY these three lines and nothing else:
            PRODUCT_ID: <UUID from above>
            QUANTITY: <integer 1–{maxQty}>
            NARRATION: <one casual sentence in first person>
            """;

        var llmResponse = await brain.ThinkAsync(persona.SystemPrompt, context, ct);

        if (!TryParseDecision(llmResponse, products, out var chosenId, out var qty, out var narration))
        {
            // Fallback: pick first product with personality-appropriate quantity.
            chosenId = products[Random.Shared.Next(products.Count)].Id;
            qty = Random.Shared.Next(persona.DefaultMinQty, persona.DefaultMaxQty + 1);
            narration = $"I'll take some {products.First(p => p.Id == chosenId).Name} — the usual.";
        }

        // ── 3. Place the order ─────────────────────────────────────────────
        var orders = factory.CreateClient("Order");
        var req = new OrderRequest(chosenId, qty, persona.Name, persona.CustomerId);
        var resp = await orders.PostAsJsonAsync("/api/orders", req, ct);

        OrderResponse? result = null;
        if (resp.IsSuccessStatusCode)
            result = await resp.Content.ReadFromJsonAsync<OrderResponse>(ct);

        if (result?.Success == true)
            await BroadcastAsync(persona, "placed-order", narration, ct);
        else
            await BroadcastAsync(persona, "order-failed",
                $"Order didn't go through: {result?.Message ?? $"HTTP {(int)resp.StatusCode}"}", ct);
    }

    private static bool TryParseDecision(
        string response,
        List<ProductItem> products,
        out Guid productId, out int qty, out string narration)
    {
        productId = Guid.Empty; qty = 1; narration = "";
        if (string.IsNullOrWhiteSpace(response)) return false;

        var lines = response.Split('\n');

        var pidLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("PRODUCT_ID:", StringComparison.OrdinalIgnoreCase));
        if (pidLine == null) return false;
        var pidStr = pidLine.Split(':', 2)[1].Trim();
        if (!Guid.TryParse(pidStr, out productId)) return false;
        var pid = productId;
        if (!products.Any(p => p.Id == pid)) return false;

        var qtyLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("QUANTITY:", StringComparison.OrdinalIgnoreCase));
        if (qtyLine == null || !int.TryParse(qtyLine.Split(':', 2)[1].Trim(), out qty) || qty < 1)
            return false;

        var narLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("NARRATION:", StringComparison.OrdinalIgnoreCase));
        narration = narLine != null ? narLine.Split(':', 2)[1].Trim() : "Just picking up a few things.";

        return true;
    }

    private async Task BroadcastAsync(Persona persona, string action, string narration, CancellationToken ct)
    {
        var activity = new AgentActivity(persona.Name, "Client", action, narration, DateTime.UtcNow);
        await hub.Clients.All.SendAsync("ReceiveActivity", activity, ct);
        logger.LogInformation("[{Agent}] {Action}: {Narration}", persona.Name, action, narration);
    }

}

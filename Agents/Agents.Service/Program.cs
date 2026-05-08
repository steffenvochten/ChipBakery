using Agents.Service;
using Agents.Service.Brain;
using Agents.Service.Hubs;
using Agents.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ── SignalR ─────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── HTTP clients for service calls (Aspire service discovery resolves URIs) ─
builder.Services.AddHttpClient("Inventory",  c => c.BaseAddress = new Uri("https://inventory-service"));
builder.Services.AddHttpClient("Order",      c => c.BaseAddress = new Uri("https://order-service"));
builder.Services.AddHttpClient("Warehouse",  c => c.BaseAddress = new Uri("https://warehouse-service"));
builder.Services.AddHttpClient("Supplier",   c => c.BaseAddress = new Uri("https://supplier-service"));
builder.Services.AddHttpClient("Production", c => c.BaseAddress = new Uri("https://production-service"));

// ── Agent settings (speed multiplier + pause flag, mutated via SignalR hub) ───
builder.Services.AddSingleton<AgentSettings>();

// ── Supplier management ──────────────────────────────────────────────────────
builder.Services.AddSingleton<ISupplierManager, SupplierManager>();

// ── Supplier agent configuration (keywords, thresholds — edit appsettings.json) ─
builder.Services.Configure<SupplierAgentOptions>(builder.Configuration);

// ── Agent brain (LLM via Ollama, degrades gracefully to rule-based fallback) ─
builder.Services.AddSingleton<IAgentBrain, OllamaAgentBrain>();

// ── Agent workers ────────────────────────────────────────────────────────────
builder.Services.AddHostedService<HeartbeatAgent>();
builder.Services.AddHostedService<ClientAgentWorker>();
builder.Services.AddHostedService<SupplierAgentWorker>();
builder.Services.AddHostedService<WarehouseManagerAgent>();
builder.Services.AddHostedService<BakerAgent>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapHub<AgentActivityHub>("/hubs/agents");

app.MapPost("/api/agents/suppliers/auto", async (
    AutoGenerateSupplierRequest request,
    IAgentBrain brain,
    ISupplierManager manager,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var logger = loggerFactory.CreateLogger("AutoSupplierApi");
    var ingredientList = string.Join(", ", request.Ingredients);
    
    var prompt = $"""
        You are a creative writer for a bakery simulation game. 
        I need a new supplier persona for the following ingredients: {ingredientList}.
        
        Generate a professional but thematic company name and a short system prompt describing their personality.
        The prompt should mention they supply {ingredientList} and take pride in their work.
        
        Reply with EXACTLY these lines and nothing else:
        NAME: <Company Name>
        PROMPT: <Short System Prompt>
        """;

    var response = await brain.ThinkAsync("You are a helpful assistant.", prompt, ct);
    
    // Robust parsing using Regex to handle conversational filler
    var nameMatch = System.Text.RegularExpressions.Regex.Match(response, @"NAME:\s*(.*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    var promptMatch = System.Text.RegularExpressions.Regex.Match(response, @"PROMPT:\s*(.*)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : $"Supplier of {request.Ingredients[0]}";
    var systemPrompt = promptMatch.Success ? promptMatch.Groups[1].Value.Trim() : $"Supplies {ingredientList}.";

    var persona = new SupplierPersona(
        name,
        systemPrompt,
        request.Ingredients.ToArray(),
        LowThreshold: 10.0m,
        DefaultRestockQty: 50.0m,
        Interval: TimeSpan.FromSeconds(Random.Shared.Next(30, 45)));

    if (manager.RegisterSupplier(persona))
    {
        logger.LogInformation("Auto-generated and registered supplier: {Name}", name);
        return Results.Ok(persona);
    }

    return Results.Conflict("A supplier with this name already exists.");
});

await app.RunAsync();

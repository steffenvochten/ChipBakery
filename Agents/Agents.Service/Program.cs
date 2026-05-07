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

await app.RunAsync();

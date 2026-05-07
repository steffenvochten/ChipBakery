using Agents.Service.Brain;
using Agents.Service.Hubs;
using Agents.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ── SignalR ─────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── HTTP clients for service calls (Aspire service discovery resolves URIs) ─
builder.Services.AddHttpClient("Inventory", c => c.BaseAddress = new Uri("https://inventory-service"));
builder.Services.AddHttpClient("Order",     c => c.BaseAddress = new Uri("https://order-service"));
builder.Services.AddHttpClient("Warehouse", c => c.BaseAddress = new Uri("https://warehouse-service"));
builder.Services.AddHttpClient("Supplier",  c => c.BaseAddress = new Uri("https://supplier-service"));

// ── Agent brain (LLM via Ollama, degrades gracefully to rule-based fallback) ─
builder.Services.AddSingleton<IAgentBrain, OllamaAgentBrain>();

// ── Agent workers ────────────────────────────────────────────────────────────
builder.Services.AddHostedService<HeartbeatAgent>();
builder.Services.AddHostedService<ClientAgentWorker>();
builder.Services.AddHostedService<SupplierAgentWorker>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapHub<AgentActivityHub>("/hubs/agents");

await app.RunAsync();

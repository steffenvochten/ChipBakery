using Agents.Service.Hubs;
using Agents.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSignalR();
builder.Services.AddHostedService<HeartbeatAgent>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapHub<AgentActivityHub>("/hubs/agents");

await app.RunAsync();

using Production.Application;
using Production.Infrastructure;
using Production.Service.Endpoints;
using Production.Service.Extensions;
using Production.Service.Middleware;
using Production.Service.Messaging;
using Production.Service.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add Clean Architecture layers.
builder.AddInfrastructure();
builder.Services.AddApplication();

// Register Messaging Consumers
builder.Services.AddHostedService<OrderPlacedConsumer>();

// Drives the baking-job lifecycle: Scheduled → Baking (after consuming ingredients)
// → Completed; jobs that find the warehouse short are held in AwaitingIngredients
// and retried on each tick once supplier deliveries have restocked.
builder.Services.AddHostedService<BakingProgressWorker>();

// Add cross-cutting concerns.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
await app.InitializeDatabaseAsync();

app.MapProductionEndpoints();

await app.RunAsync();

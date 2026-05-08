using Order.Application;
using Order.Infrastructure;
using Order.Service.Endpoints;
using Order.Service.Extensions;
using Order.Service.Middleware;
using Order.Service.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ─── Service Registration ──────────────────────────────────────────────────────

builder.AddServiceDefaults();

// Infrastructure: DbContext (Aspire Postgres integration), Repository, EventPublisher, InventoryClient
builder.AddInfrastructure();

// Application: OrderService, FluentValidation validators
builder.Services.AddApplication();

// Messaging Consumers
builder.Services.AddHostedService<JobEventConsumer>();

// ProblemDetails support for RFC 7807-compliant error responses
builder.Services.AddProblemDetails();

// Register the global exception handler (maps domain exceptions → ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ─── Middleware Pipeline ───────────────────────────────────────────────────────

var app = builder.Build();

app.MapDefaultEndpoints();          // Aspire health & aliveness endpoints
app.UseExceptionHandler();          // Must come before endpoint mapping

// ─── Database Initialization ───────────────────────────────────────────────────

await app.InitializeDatabaseAsync();

// ─── Endpoints ────────────────────────────────────────────────────────────────

app.MapOrderEndpoints();

await app.RunAsync();

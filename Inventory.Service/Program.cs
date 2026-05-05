using Inventory.Application;
using Inventory.Infrastructure;
using Inventory.Service.Endpoints;
using Inventory.Service.Extensions;
using Inventory.Service.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ─── Service Registration ──────────────────────────────────────────────────────

builder.AddServiceDefaults();

// Infrastructure: DbContext (Aspire Postgres integration), Repository, EventPublisher
builder.AddInfrastructure();

// Application: InventoryService, FluentValidation validators
builder.Services.AddApplication();

// ProblemDetails support for RFC 7807-compliant error responses
builder.Services.AddProblemDetails();

// Register the global exception handler (maps domain exceptions → ProblemDetails)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ─── Middleware Pipeline ───────────────────────────────────────────────────────

var app = builder.Build();

app.MapDefaultEndpoints();          // Aspire health & aliveness endpoints
app.UseExceptionHandler();          // Must come before endpoint mapping

// ─── Database Initialization ───────────────────────────────────────────────────

app.InitializeDatabase();

// ─── Endpoints ────────────────────────────────────────────────────────────────

app.MapInventoryEndpoints();

app.Run();

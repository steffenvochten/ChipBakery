using Production.Application;
using Production.Infrastructure;
using Production.Service.Endpoints;
using Production.Service.Extensions;
using Production.Service.Middleware;
using Production.Service.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add Clean Architecture layers.
builder.AddInfrastructure();
builder.Services.AddApplication();

// Register Messaging Consumers
builder.Services.AddHostedService<OrderPlacedConsumer>();

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

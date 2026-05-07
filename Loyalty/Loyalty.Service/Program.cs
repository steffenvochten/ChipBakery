using Loyalty.Application;
using Loyalty.Infrastructure;
using Loyalty.Service.Endpoints;
using Loyalty.Service.Extensions;
using Loyalty.Service.Messaging;
using Loyalty.Service.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add Clean Architecture layers.
builder.AddInfrastructure();
builder.Services.AddApplication();

// Add cross-cutting concerns.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

// RabbitMQ consumers
builder.Services.AddHostedService<OrderPlacedConsumer>();
builder.Services.AddHostedService<JobCompletedConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
await app.InitializeDatabaseAsync();

app.MapLoyaltyEndpoints();

await app.RunAsync();

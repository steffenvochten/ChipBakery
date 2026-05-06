using Warehouse.Application;
using Warehouse.Infrastructure;
using Warehouse.Service.Endpoints;
using Warehouse.Service.Extensions;
using Warehouse.Service.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add Clean Architecture layers.
builder.AddInfrastructure();
builder.Services.AddApplication();

// Add Web API services.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.InitializeDatabase();
app.MapWarehouseEndpoints();

app.Run();

using Supplier.Application;
using Supplier.Infrastructure;
using Supplier.Service.Endpoints;
using Supplier.Service.Extensions;
using Supplier.Service.Middleware;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.InitializeDatabase();

app.MapSupplierEndpoints();

app.Run();

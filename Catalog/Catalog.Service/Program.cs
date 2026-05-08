using Catalog.Infrastructure;
using Catalog.Service;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddInfrastructure();

builder.Services.AddHttpClient("Inventory",  c => c.BaseAddress = new Uri("https://inventory-service"));
builder.Services.AddHttpClient("Warehouse",  c => c.BaseAddress = new Uri("https://warehouse-service"));
builder.Services.AddHttpClient("Agents",     c => c.BaseAddress = new Uri("https://agents-service"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCatalogEndpoints();

app.Run();

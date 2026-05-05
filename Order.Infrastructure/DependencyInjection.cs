using ChipBakery.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Application.Interfaces;
using Order.Domain.Interfaces;
using Order.Infrastructure.Clients;
using Order.Infrastructure.Events;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Persistence.Repositories;

namespace Order.Infrastructure;

/// <summary>
/// Extension method to register all Infrastructure-layer services into the DI container.
/// Call this from the API project's composition root (Program.cs) via the IHostApplicationBuilder overload
/// so the Aspire AddNpgsqlDbContext integration is available.
/// </summary>
public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Register the EF Core DbContext using the Aspire Npgsql integration.
        // "orderdb" matches the database name registered in ChipBakery.AppHost/Program.cs.
        builder.AddNpgsqlDbContext<OrderDbContext>("orderdb");

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();

        // TODO: Swap MockEventPublisher for RabbitMqEventPublisher when ready.
        // See Order.Infrastructure/Events/MockEventPublisher.cs for the replacement guide.
        builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();

        // Named HttpClient for Inventory.Service.
        // "https+http://inventory-service" is resolved by Aspire service discovery at runtime.
        // The name "Inventory" is used by HttpInventoryClient via IHttpClientFactory.CreateClient("Inventory").
        builder.Services.AddHttpClient("Inventory", client =>
            client.BaseAddress = new Uri("https+http://inventory-service"));

        builder.Services.AddScoped<IInventoryClient, HttpInventoryClient>();

        return builder;
    }
}

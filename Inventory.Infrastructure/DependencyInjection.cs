using ChipBakery.Shared;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Events;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Inventory.Infrastructure;

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
        // "inventorydb" matches the database name registered in ChipBakery.AppHost/Program.cs.
        builder.AddNpgsqlDbContext<InventoryDbContext>("inventorydb");

        builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

        // TODO: Swap MockEventPublisher for RabbitMqEventPublisher when ready.
        // See Inventory.Infrastructure/Events/MockEventPublisher.cs for the replacement guide.
        builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();

        return builder;
    }
}

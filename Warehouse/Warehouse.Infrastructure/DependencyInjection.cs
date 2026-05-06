using ChipBakery.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warehouse.Domain.Interfaces;
using Warehouse.Infrastructure.Events;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Infrastructure.Persistence.Repositories;

namespace Warehouse.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<WarehouseDbContext>("warehousedb");
        builder.Services.AddScoped<IWarehouseRepository, WarehouseItemRepository>();
        
        // RabbitMQ Event Publisher integration
        builder.AddRabbitMQClient("rabbitmq");
        builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        
        return builder;
    }
}

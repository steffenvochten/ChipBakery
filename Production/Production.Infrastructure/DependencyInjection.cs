using ChipBakery.Shared;
using Production.Application.Interfaces;
using Production.Domain.Interfaces;
using Production.Infrastructure.Clients;
using Production.Infrastructure.Persistence;
using Production.Infrastructure.Persistence.Repositories;
using Production.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Production.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<ProductionDbContext>("productiondb");
        builder.Services.AddScoped<IBakingJobRepository, BakingJobRepository>();

        // Redis for real-time tracking
        builder.AddRedisClient("redis");
        builder.Services.AddScoped<ITrackingService, RedisTrackingService>();

        // RabbitMQ Event Publisher integration
        builder.AddRabbitMQClient("rabbitmq");
        builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        // Named HttpClient for Warehouse.Service — used by BakingService when starting a job
        // to atomically check and deduct ingredients. "https://warehouse-service" is resolved
        // by Aspire service discovery at runtime.
        builder.Services.AddHttpClient("Warehouse", client =>
            client.BaseAddress = new Uri("https://warehouse-service"));
        builder.Services.AddScoped<IWarehouseClient, HttpWarehouseClient>();

        return builder;
    }
}

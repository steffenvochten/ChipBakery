using ChipBakery.Shared;
using Production.Domain.Interfaces;
using Production.Infrastructure.Events;
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
        
        return builder;
    }
}

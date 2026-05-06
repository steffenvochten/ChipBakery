using ChipBakery.Shared;
using Production.Domain.Interfaces;
using Production.Infrastructure.Events;
using Production.Infrastructure.Persistence;
using Production.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Production.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<ProductionDbContext>("productiondb");
        builder.Services.AddScoped<IBakingScheduleRepository, BakingScheduleRepository>();
        builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();
        return builder;
    }
}

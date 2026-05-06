using ChipBakery.Shared;
using Loyalty.Domain.Interfaces;
using Loyalty.Infrastructure.Events;
using Loyalty.Infrastructure.Persistence;
using Loyalty.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Loyalty.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<LoyaltyDbContext>("loyaltydb");
        builder.Services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();
        return builder;
    }
}

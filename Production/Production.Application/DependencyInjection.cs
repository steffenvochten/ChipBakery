using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Production.Application.Interfaces;
using Production.Application.Services;

namespace Production.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBakingScheduleService, BakingScheduleService>();
        services.AddValidatorsFromAssemblyContaining<BakingScheduleService>();
        return services;
    }
}

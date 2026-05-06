using FluentValidation;
using Loyalty.Application.Interfaces;
using Loyalty.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Loyalty.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddValidatorsFromAssemblyContaining<LoyaltyService>();
        return services;
    }
}

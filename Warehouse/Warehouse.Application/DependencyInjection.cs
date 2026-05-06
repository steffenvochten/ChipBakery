using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Services;

namespace Warehouse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddValidatorsFromAssemblyContaining<WarehouseService>();
        return services;
    }
}

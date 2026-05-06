using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Supplier.Application.Interfaces;
using Supplier.Application.Services;

namespace Supplier.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddValidatorsFromAssemblyContaining<SupplierService>();
        return services;
    }
}

using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICatalogService, CatalogService>();
        
        return services;
    }
}

using FluentValidation;
using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application;

/// <summary>
/// Extension method to register all Application-layer services into the DI container.
/// Call this from the API project's composition root (Program.cs).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IInventoryService, InventoryService>();

        // Register all FluentValidation validators from this assembly automatically
        services.AddValidatorsFromAssemblyContaining<InventoryService>();

        return services;
    }
}

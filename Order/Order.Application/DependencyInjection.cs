using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Interfaces;
using Order.Application.Services;

namespace Order.Application;

/// <summary>
/// Extension method to register all Application-layer services into the DI container.
/// Call this from the API project's composition root (Program.cs).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();

        // Register all FluentValidation validators from this assembly automatically.
        // Discovers PlaceOrderValidator (and any future validators) without manual registration.
        services.AddValidatorsFromAssemblyContaining<OrderService>();

        return services;
    }
}

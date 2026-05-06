using ChipBakery.Shared;
using Supplier.Domain.Interfaces;
using Supplier.Infrastructure.Events;
using Supplier.Infrastructure.Persistence;
using Supplier.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Supplier.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<SupplierDbContext>("supplierdb");
        builder.Services.AddScoped<IIngredientSupplyRepository, IngredientSupplyRepository>();
        builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();
        return builder;
    }
}

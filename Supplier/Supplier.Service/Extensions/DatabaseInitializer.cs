using Microsoft.EntityFrameworkCore;
using Supplier.Infrastructure.Persistence;

namespace Supplier.Service.Extensions;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        await using var scope = app.ApplicationServices.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
        await context.Database.MigrateAsync();
    }
}

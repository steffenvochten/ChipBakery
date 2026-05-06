using Microsoft.EntityFrameworkCore;
using Supplier.Infrastructure.Persistence;

namespace Supplier.Service.Extensions;

public static class DatabaseInitializer
{
    public static void InitializeDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SupplierDbContext>();
        
        // Apply any pending migrations
        context.Database.Migrate();
    }
}

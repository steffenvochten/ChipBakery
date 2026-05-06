using Microsoft.EntityFrameworkCore;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Service.Extensions;

public static class DatabaseInitializer
{
    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        db.Database.Migrate();
        WarehouseSeedData.SeedAsync(db).GetAwaiter().GetResult();
        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Service.Extensions;

public static class DatabaseInitializer
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        await db.Database.MigrateAsync();
        await WarehouseSeedData.SeedAsync(db);
        return app;
    }
}

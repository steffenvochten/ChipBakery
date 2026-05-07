using Microsoft.EntityFrameworkCore;
using Loyalty.Infrastructure.Persistence;

namespace Loyalty.Service.Extensions;

public static class DatabaseInitializer
{
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
        await db.Database.MigrateAsync();
        return app;
    }
}

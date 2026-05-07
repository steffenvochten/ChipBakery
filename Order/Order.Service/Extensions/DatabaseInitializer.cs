using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Persistence;

namespace Order.Service.Extensions;

/// <summary>
/// Handles database initialization on application startup.
/// Extracted from Program.cs to keep the composition root clean.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Ensures the database schema is created and seed data is applied.
    /// Called once during startup before the app begins accepting requests.
    /// </summary>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        await db.Database.MigrateAsync();

        return app;
    }
}

using Microsoft.EntityFrameworkCore;
using Inventory.Infrastructure.Persistence;

namespace Inventory.Service.Extensions;

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
    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        db.Database.Migrate();

        return app;
    }
}

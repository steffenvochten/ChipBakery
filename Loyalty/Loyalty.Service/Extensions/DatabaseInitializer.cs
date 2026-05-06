using Microsoft.EntityFrameworkCore;
using Loyalty.Infrastructure.Persistence;

namespace Loyalty.Service.Extensions;

public static class DatabaseInitializer
{
    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
        db.Database.Migrate();
        return app;
    }
}

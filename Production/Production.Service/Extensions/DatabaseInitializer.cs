using Microsoft.EntityFrameworkCore;
using Production.Infrastructure.Persistence;


namespace Production.Service.Extensions;

public static class DatabaseInitializer
{
    public static void InitializeDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();
        
        // Ensure database is created and migrations are applied
        context.Database.Migrate();
    }
}

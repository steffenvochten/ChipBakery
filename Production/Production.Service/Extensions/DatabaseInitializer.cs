using Microsoft.EntityFrameworkCore;
using Production.Infrastructure.Persistence;


namespace Production.Service.Extensions;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        await using var scope = app.ApplicationServices.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductionDbContext>();
        await context.Database.MigrateAsync();
    }
}

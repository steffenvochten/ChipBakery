using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Production.Infrastructure.Persistence;

public class ProductionDbContextFactory : IDesignTimeDbContextFactory<ProductionDbContext>
{
    public ProductionDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ProductionDbContext>()
            .UseNpgsql("Host=localhost;Database=productiondb;Username=postgres;Password=postgres")
            .Options;
        return new ProductionDbContext(options);
    }
}

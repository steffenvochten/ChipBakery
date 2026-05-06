using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Warehouse.Infrastructure.Persistence;

public class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseNpgsql("Host=localhost;Database=warehousedb;Username=postgres;Password=postgres")
            .Options;
        return new WarehouseDbContext(options);
    }
}

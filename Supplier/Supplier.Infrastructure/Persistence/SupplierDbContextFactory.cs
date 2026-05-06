using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Supplier.Infrastructure.Persistence;

public class SupplierDbContextFactory : IDesignTimeDbContextFactory<SupplierDbContext>
{
    public SupplierDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseNpgsql("Host=localhost;Database=supplierdb;Username=postgres;Password=postgres")
            .Options;
        return new SupplierDbContext(options);
    }
}

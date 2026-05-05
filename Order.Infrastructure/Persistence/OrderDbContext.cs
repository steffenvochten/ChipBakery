using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.Persistence.Configurations;

namespace Order.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Order service.
/// Schema is controlled entirely by <see cref="BakeryOrderConfiguration"/>.
/// </summary>
public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<BakeryOrder> Orders => Set<BakeryOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply the entity configuration from a separate class to keep this clean
        modelBuilder.ApplyConfiguration(new BakeryOrderConfiguration());
    }
}

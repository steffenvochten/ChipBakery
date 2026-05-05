using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> Items => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply the entity configuration from a separate class to keep this clean
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
    }
}

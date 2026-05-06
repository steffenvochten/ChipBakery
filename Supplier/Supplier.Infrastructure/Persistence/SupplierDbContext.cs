using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;
using Supplier.Infrastructure.Persistence.Configurations;

namespace Supplier.Infrastructure.Persistence;

public class SupplierDbContext(DbContextOptions<SupplierDbContext> options) : DbContext(options)
{
    public DbSet<IngredientSupply> IngredientSupplies => Set<IngredientSupply>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new IngredientSupplyConfiguration());
    }
}

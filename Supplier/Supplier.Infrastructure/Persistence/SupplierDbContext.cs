using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;

namespace Supplier.Infrastructure.Persistence;

public class SupplierDbContext(DbContextOptions<SupplierDbContext> options) : DbContext(options)
{
    public DbSet<SupplierTransport> SupplierTransports => Set<SupplierTransport>();
    public DbSet<IngredientSupply> IngredientSupplies => Set<IngredientSupply>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupplierDbContext).Assembly);
    }
}

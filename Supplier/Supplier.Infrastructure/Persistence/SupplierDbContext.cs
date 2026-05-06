using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;

namespace Supplier.Infrastructure.Persistence;

public class SupplierDbContext(DbContextOptions<SupplierDbContext> options) : DbContext(options)
{
    public DbSet<SupplierTransport> SupplierTransports => Set<SupplierTransport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SupplierTransport>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.IngredientName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Unit).IsRequired().HasMaxLength(50);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supplier.Domain.Entities;

namespace Supplier.Infrastructure.Persistence.Configurations;

public class IngredientSupplyConfiguration : IEntityTypeConfiguration<IngredientSupply>
{
    public void Configure(EntityTypeBuilder<IngredientSupply> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IngredientName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SupplierName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.ScheduledDate).IsRequired();

        // Seed data with stable GUIDs
        builder.HasData(
            new IngredientSupply
            {
                Id = Guid.Parse("f2a1b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c"),
                IngredientName = "Flour",
                SupplierName = "Grain Millers",
                Quantity = 1000,
                Price = 500.00m,
                ScheduledDate = DateTime.UtcNow.AddDays(1)
            },
            new IngredientSupply
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                IngredientName = "Yeast",
                SupplierName = "Bakers Best",
                Quantity = 100,
                Price = 200.00m,
                ScheduledDate = DateTime.UtcNow.AddDays(2)
            }
        );
    }
}

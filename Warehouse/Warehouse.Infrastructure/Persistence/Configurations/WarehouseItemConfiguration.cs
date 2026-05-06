using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Persistence.Configurations;

public class WarehouseItemConfiguration : IEntityTypeConfiguration<WarehouseItem>
{
    public void Configure(EntityTypeBuilder<WarehouseItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(i => i.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasData(
            new WarehouseItem
            {
                Id = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                Name = "Bread Flour",
                Quantity = 500,
                Unit = "kg"
            },
            new WarehouseItem
            {
                Id = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                Name = "Yeast",
                Quantity = 10,
                Unit = "kg"
            },
            new WarehouseItem
            {
                Id = Guid.Parse("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                Name = "Butter",
                Quantity = 100,
                Unit = "kg"
            },
            new WarehouseItem
            {
                Id = Guid.Parse("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"),
                Name = "Milk",
                Quantity = 200,
                Unit = "liters"
            }
        );
    }
}

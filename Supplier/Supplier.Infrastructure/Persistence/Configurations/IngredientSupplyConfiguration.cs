using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supplier.Domain.Entities;

namespace Supplier.Infrastructure.Persistence.Configurations;

public class IngredientSupplyConfiguration : IEntityTypeConfiguration<IngredientSupply>
{
    public void Configure(EntityTypeBuilder<IngredientSupply> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IngredientName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SupplierName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.ScheduledDate)
            .IsRequired();
    }
}

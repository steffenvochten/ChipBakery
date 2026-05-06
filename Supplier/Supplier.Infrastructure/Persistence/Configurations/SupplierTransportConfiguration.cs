using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supplier.Domain.Entities;

namespace Supplier.Infrastructure.Persistence.Configurations;

public class SupplierTransportConfiguration : IEntityTypeConfiguration<SupplierTransport>
{
    public void Configure(EntityTypeBuilder<SupplierTransport> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IngredientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Timestamp)
            .IsRequired();
    }
}

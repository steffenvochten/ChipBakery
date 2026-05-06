using Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyalty.Infrastructure.Persistence.Configurations;

public class CustomerLoyaltyConfiguration : IEntityTypeConfiguration<CustomerLoyalty>
{
    public void Configure(EntityTypeBuilder<CustomerLoyalty> builder)
    {
        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.TotalPoints)
            .IsRequired();

        builder.Property(x => x.Tier)
            .IsRequired()
            .HasConversion<string>();
    }
}

public class LoyaltyTransactionConfiguration : IEntityTypeConfiguration<LoyaltyTransaction>
{
    public void Configure(EntityTypeBuilder<LoyaltyTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}

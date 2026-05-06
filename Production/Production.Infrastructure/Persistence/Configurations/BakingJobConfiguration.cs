using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Entities;

namespace Production.Infrastructure.Persistence.Configurations;

public class BakingJobConfiguration : IEntityTypeConfiguration<BakingJob>
{
    public void Configure(EntityTypeBuilder<BakingJob> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);
            
        builder.Property(x => x.StartTime);
        builder.Property(x => x.EndTime);
    }
}

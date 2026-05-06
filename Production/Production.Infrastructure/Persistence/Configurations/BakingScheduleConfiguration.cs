using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Entities;

namespace Production.Infrastructure.Persistence.Configurations;

public class BakingScheduleConfiguration : IEntityTypeConfiguration<BakingSchedule>
{
    public void Configure(EntityTypeBuilder<BakingSchedule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ScheduledTime)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();
    }
}

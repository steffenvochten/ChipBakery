using Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loyalty.Infrastructure.Persistence.Configurations;

public class LoyaltyMemberConfiguration : IEntityTypeConfiguration<LoyaltyMember>
{
    public void Configure(EntityTypeBuilder<LoyaltyMember> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.Property(x => x.Points).IsRequired();

        builder.HasIndex(x => x.CustomerName).IsUnique();

        builder.HasData(
            new LoyaltyMember
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-a7b8-c9d0-e1f2a3b4c5d6"),
                CustomerName = "John Doe",
                Email = "john.doe@example.com",
                Points = 1000
            },
            new LoyaltyMember
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-b8c9-d0e1-f2a3b4c5d6e7"),
                CustomerName = "Jane Smith",
                Email = "jane.smith@example.com",
                Points = 2500
            },
            new LoyaltyMember
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-c9d0-e1f2-a3b4c5d6e7f8"),
                CustomerName = "Bob Wilson",
                Email = "bob.wilson@example.com",
                Points = 500
            }
        );
    }
}

using Loyalty.Domain.Entities;
using Loyalty.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Infrastructure.Persistence;

public class LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : DbContext(options)
{
    public DbSet<LoyaltyMember> Members => Set<LoyaltyMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new LoyaltyMemberConfiguration());
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Loyalty.Infrastructure.Persistence;

public class LoyaltyDbContextFactory : IDesignTimeDbContextFactory<LoyaltyDbContext>
{
    public LoyaltyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LoyaltyDbContext>()
            .UseNpgsql("Host=localhost;Database=loyaltydb;Username=postgres;Password=postgres")
            .Options;
        return new LoyaltyDbContext(options);
    }
}

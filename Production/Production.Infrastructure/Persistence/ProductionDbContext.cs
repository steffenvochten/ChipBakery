using Microsoft.EntityFrameworkCore;
using Production.Domain.Entities;
using Production.Infrastructure.Persistence.Configurations;

namespace Production.Infrastructure.Persistence;

public class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options)
{
    public DbSet<BakingJob> BakingJobs => Set<BakingJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BakingJobConfiguration());
    }
}

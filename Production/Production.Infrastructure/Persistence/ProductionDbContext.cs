using Microsoft.EntityFrameworkCore;
using Production.Domain.Entities;
using Production.Infrastructure.Persistence.Configurations;

namespace Production.Infrastructure.Persistence;

public class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options)
{
    public DbSet<BakingSchedule> BakingSchedules => Set<BakingSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new BakingScheduleConfiguration());
    }
}

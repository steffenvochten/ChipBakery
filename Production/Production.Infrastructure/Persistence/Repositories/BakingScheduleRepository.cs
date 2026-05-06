using Microsoft.EntityFrameworkCore;
using Production.Domain.Entities;
using Production.Domain.Interfaces;

namespace Production.Infrastructure.Persistence.Repositories;

public class BakingScheduleRepository(ProductionDbContext context) : IBakingScheduleRepository
{
    private readonly ProductionDbContext _context = context;

    public async Task<BakingSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BakingSchedules.FindAsync([id], ct);
    }

    public async Task<List<BakingSchedule>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.BakingSchedules
            .AsNoTracking()
            .OrderByDescending(x => x.ScheduledTime)
            .ToListAsync(ct);
    }

    public async Task AddAsync(BakingSchedule item, CancellationToken ct = default)
    {
        await _context.BakingSchedules.AddAsync(item, ct);
    }

    public void Update(BakingSchedule item)
    {
        _context.BakingSchedules.Update(item);
    }

    public void Delete(BakingSchedule item)
    {
        _context.BakingSchedules.Remove(item);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}

using Microsoft.EntityFrameworkCore;
using Production.Domain.Entities;
using Production.Domain.Interfaces;

namespace Production.Infrastructure.Persistence.Repositories;

public class BakingJobRepository : IBakingJobRepository
{
    private readonly ProductionDbContext _context;

    public BakingJobRepository(ProductionDbContext context)
    {
        _context = context;
    }

    public async Task<BakingJob?> GetByIdAsync(Guid id)
    {
        return await _context.BakingJobs.FindAsync(id);
    }

    public async Task<IEnumerable<BakingJob>> GetAllAsync()
    {
        return await _context.BakingJobs.ToListAsync();
    }

    public async Task<IEnumerable<BakingJob>> GetByStatusAsync(string status)
    {
        return await _context.BakingJobs
            .Where(j => j.Status == status)
            .ToListAsync();
    }

    public async Task AddAsync(BakingJob job)
    {
        await _context.BakingJobs.AddAsync(job);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BakingJob job)
    {
        _context.BakingJobs.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await GetByIdAsync(id);
        if (job != null)
        {
            _context.BakingJobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
}

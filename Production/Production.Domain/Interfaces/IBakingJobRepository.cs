using Production.Domain.Entities;

namespace Production.Domain.Interfaces;

public interface IBakingJobRepository
{
    Task<BakingJob?> GetByIdAsync(Guid id);
    Task<IEnumerable<BakingJob>> GetAllAsync();
    Task AddAsync(BakingJob job);
    Task UpdateAsync(BakingJob job);
}

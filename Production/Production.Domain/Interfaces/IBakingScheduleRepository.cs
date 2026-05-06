using Production.Domain.Entities;

namespace Production.Domain.Interfaces;

public interface IBakingScheduleRepository
{
    Task<BakingSchedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BakingSchedule>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(BakingSchedule item, CancellationToken ct = default);
    void Update(BakingSchedule item);
    void Delete(BakingSchedule item);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

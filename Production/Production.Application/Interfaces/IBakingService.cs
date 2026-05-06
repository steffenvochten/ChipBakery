using Production.Application.DTOs;

namespace Production.Application.Interfaces;

public interface IBakingService
{
    Task<List<BakingJobDto>> GetAllJobsAsync(CancellationToken ct = default);
    Task<BakingJobDto> ScheduleJobAsync(ScheduleBakingJobRequest request, CancellationToken ct = default);
    Task StartJobAsync(Guid jobId, CancellationToken ct = default);
    Task CompleteJobAsync(Guid jobId, CancellationToken ct = default);
}

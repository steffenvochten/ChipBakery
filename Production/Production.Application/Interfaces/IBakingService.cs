using Production.Application.DTOs;

namespace Production.Application.Interfaces;

public interface IBakingService
{
    Task<List<BakingJobDto>> GetAllJobsAsync(CancellationToken ct = default);
    Task<BakingJobDto> ScheduleJobAsync(ScheduleBakingJobRequest request, CancellationToken ct = default);
    Task StartJobAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>
    /// Attempts to advance a job from Scheduled/AwaitingIngredients into Baking,
    /// returning true on success. Used by the BakingProgressWorker to drive the lifecycle.
    /// </summary>
    Task<bool> TryStartJobAsync(Guid jobId, CancellationToken ct = default);

    Task CompleteJobAsync(Guid jobId, CancellationToken ct = default);

    Task<List<BakingJobDto>> GetJobsByStatusAsync(string status, CancellationToken ct = default);
}

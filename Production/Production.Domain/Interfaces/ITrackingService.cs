namespace Production.Domain.Interfaces;

public interface ITrackingService
{
    Task UpdateJobStatusAsync(Guid jobId, string status);
}

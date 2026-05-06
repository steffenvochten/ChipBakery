using Microsoft.Extensions.Logging;
using Production.Domain.Interfaces;

namespace Production.Infrastructure.Services;

public class MockTrackingService : ITrackingService
{
    private readonly ILogger<MockTrackingService> _logger;

    public MockTrackingService(ILogger<MockTrackingService> logger)
    {
        _logger = logger;
    }

    public Task UpdateJobStatusAsync(Guid jobId, string status)
    {
        _logger.LogInformation("REAL-TIME TRACKING: Job {JobId} status updated to {Status}", jobId, status);
        return Task.CompletedTask;
    }
}

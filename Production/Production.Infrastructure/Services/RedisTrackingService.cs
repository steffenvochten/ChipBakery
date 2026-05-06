using Production.Domain.Interfaces;
using StackExchange.Redis;

namespace Production.Infrastructure.Services;

public class RedisTrackingService : ITrackingService
{
    private readonly IDatabase _db;

    public RedisTrackingService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task UpdateJobStatusAsync(Guid jobId, string status)
    {
        // Store as a hash for easy retrieval
        await _db.HashSetAsync($"production:job:{jobId}", new HashEntry[] {
            new HashEntry("status", status),
            new HashEntry("updatedAt", DateTime.UtcNow.ToString("O"))
        });
        
        // Also add to a global list of active jobs if needed
        await _db.SetAddAsync("production:active-jobs", jobId.ToString());
    }
}

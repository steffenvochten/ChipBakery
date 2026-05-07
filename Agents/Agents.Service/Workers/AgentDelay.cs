namespace Agents.Service.Workers;

/// <summary>
/// Drop-in replacement for Task.Delay that respects the live
/// <see cref="AgentSettings.SpeedMultiplier"/> and
/// <see cref="AgentSettings.IsPaused"/> flag.
/// </summary>
internal static class AgentDelay
{
    /// <summary>
    /// Waits for <paramref name="baseDelay"/> / SpeedMultiplier, checking every 100 ms
    /// whether the agents are paused (in which case the countdown freezes until resumed).
    /// </summary>
    public static async Task SmartAsync(
        TimeSpan baseDelay, AgentSettings settings, CancellationToken ct)
    {
        try
        {
            // Compute the adjusted target duration from the current speed at the start of
            // each delay period. A mid-delay speed change takes effect on the next tick.
            var effective = TimeSpan.FromTicks(
                (long)(baseDelay.Ticks / Math.Max(0.1, settings.SpeedMultiplier)));

            var deadline = DateTime.UtcNow + effective;

            while (!ct.IsCancellationRequested)
            {
                if (settings.IsPaused)
                {
                    // Freeze: push the deadline forward so elapsed time doesn't count
                    deadline = DateTime.UtcNow + (deadline - DateTime.UtcNow);
                    await Task.Delay(100, ct);
                    continue;
                }

                if (DateTime.UtcNow >= deadline) break;

                var remaining = deadline - DateTime.UtcNow;
                await Task.Delay(
                    TimeSpan.FromMilliseconds(Math.Min(100, remaining.TotalMilliseconds)), ct);
            }
        }
        catch (TaskCanceledException) { }
    }
}

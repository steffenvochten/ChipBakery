namespace Agents.Service;

/// <summary>
/// Singleton that controls the live behaviour of all agent workers.
/// The AgentActivityHub exposes hub methods so the Web frontend can
/// mutate these values in real time.
/// </summary>
public sealed class AgentSettings
{
    private double _speedMultiplier = 1.0;

    /// <summary>
    /// Divides every agent's base tick interval.
    /// 1.0 = normal speed, 2.0 = twice as fast, 0.5 = half speed.
    /// Clamped to [0.25, 10].
    /// </summary>
    public double SpeedMultiplier
    {
        get => _speedMultiplier;
        set => _speedMultiplier = Math.Clamp(value, 0.25, 10.0);
    }

    /// <summary>When true all agent loops pause between ticks.</summary>
    public bool IsPaused { get; set; } = false;
}

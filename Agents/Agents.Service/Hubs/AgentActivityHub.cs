using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Hubs;

public class AgentActivityHub(AgentSettings settings) : Hub
{
    /// <summary>Called by the Web frontend slider to change agent tick speed.</summary>
    public void SetSpeed(double multiplier) => settings.SpeedMultiplier = multiplier;

    /// <summary>Called by the Web frontend pause/resume button.</summary>
    public void SetPaused(bool paused) => settings.IsPaused = paused;
}

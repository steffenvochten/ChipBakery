using Agents.Service.Hubs;
using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Agents.Service.Workers;

public class HeartbeatAgent(
    IHubContext<AgentActivityHub> hub,
    ILogger<HeartbeatAgent> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    private static readonly string[] Messages =
    [
        "Bakery systems nominal. All agents standing by.",
        "Ovens warm, ingredients stocked. Ready to bake.",
        "All systems operational. Watching the bakery.",
        "Monitoring order queue. Everything looks good.",
        "ChipBakery agent network is live."
    ];

    private int _tick = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("HeartbeatAgent started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var activity = new AgentActivity(
                AgentName: "System",
                AgentType: "Heartbeat",
                Action: "heartbeat",
                Narration: Messages[_tick % Messages.Length],
                Timestamp: DateTime.UtcNow);

            await hub.Clients.All.SendAsync("ReceiveActivity", activity, stoppingToken);
            _tick++;

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException) { /* shutting down */ }
        }
    }
}

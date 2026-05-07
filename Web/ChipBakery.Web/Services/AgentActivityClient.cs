using ChipBakery.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChipBakery.Web.Services;

/// <summary>
/// Singleton SignalR client that maintains a connection to the AgentActivityHub
/// and keeps a rolling list of the most recent agent activities.
/// Components subscribe to <see cref="OnChange"/> and call StateHasChanged when it fires.
/// </summary>
public sealed class AgentActivityClient : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly List<AgentActivity> _activities = [];
    private readonly Lock _lock = new();

    public event Action? OnChange;

    public IReadOnlyList<AgentActivity> Activities
    {
        get { lock (_lock) { return [.. _activities]; } }
    }

    public HubConnectionState State => _connection.State;

    public AgentActivityClient(IConfiguration configuration)
    {
        // Aspire injects the resolved service URL via environment variable
        // services__agents-service__https__0 (mapped as services:agents-service:https:0 in IConfiguration).
        var agentsUrl = configuration["services:agents-service:https:0"]
                        ?? configuration["services:agents-service:http:0"]
                        ?? "https://localhost:17350";

        _connection = new HubConnectionBuilder()
            .WithUrl($"{agentsUrl}/hubs/agents", o =>
            {
                // Trust dev certificates for inter-service TLS in local Aspire environment.
                o.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<AgentActivity>("ReceiveActivity", activity =>
        {
            lock (_lock)
            {
                _activities.Insert(0, activity);
                if (_activities.Count > 200)
                    _activities.RemoveAt(_activities.Count - 1);
            }
            OnChange?.Invoke();
        });
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync(ct);
    }

    public Task SetSpeedAsync(double multiplier) =>
        _connection.State == HubConnectionState.Connected
            ? _connection.InvokeAsync("SetSpeed", multiplier)
            : Task.CompletedTask;

    public Task SetPausedAsync(bool paused) =>
        _connection.State == HubConnectionState.Connected
            ? _connection.InvokeAsync("SetPaused", paused)
            : Task.CompletedTask;

    public void ClearActivities()
    {
        lock (_lock) { _activities.Clear(); }
        OnChange?.Invoke();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}

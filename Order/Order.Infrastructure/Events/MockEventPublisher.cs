using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;

namespace Order.Infrastructure.Events;

// ┌─────────────────────────────────────────────────────────────────────────────┐
// │                        MOCK IMPLEMENTATION                                  │
// │                                                                             │
// │  This publisher logs events as structured JSON instead of sending them to   │
// │  a real message broker. It is intentionally kept simple so the rest of the  │
// │  architecture (IEventPublisher abstraction, event records, etc.) is already  │
// │  wired up and production-ready.                                             │
// │                                                                             │
// │  TO REPLACE WITH RABBITMQ / MASSTRANSIT:                                    │
// │  1. Add NuGet: Aspire.RabbitMQ.Client (or MassTransit.RabbitMQ)            │
// │  2. In AppHost/Program.cs, orderService already has .WithReference(rabbitmq) │
// │  3. Create RabbitMqEventPublisher : IEventPublisher that uses               │
// │     IConnection / IModel (plain AMQP) or IBus (MassTransit).               │
// │  4. In DependencyInjection.cs swap the registration:                        │
// │         builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();│
// └─────────────────────────────────────────────────────────────────────────────┘

/// <summary>
/// Development/placeholder implementation of <see cref="IEventPublisher"/>.
/// Serializes events to JSON and emits them as structured log entries so they
/// can be observed in the Aspire dashboard's telemetry view.
/// </summary>
public class MockEventPublisher : IEventPublisher
{
    private readonly ILogger<MockEventPublisher> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    public MockEventPublisher(ILogger<MockEventPublisher> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        var eventName = typeof(TEvent).Name;
        var payload = JsonSerializer.Serialize(@event, _jsonOptions);

        _logger.LogInformation(
            "[MOCK EVENT BUS] Published {EventName}: {Payload}",
            eventName,
            payload);

        return Task.CompletedTask;
    }
}

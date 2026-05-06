using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;

namespace Supplier.Infrastructure.Events;

// TODO: Replace with RabbitMQ/MassTransit publisher.
public class MockEventPublisher(ILogger<MockEventPublisher> logger) : IEventPublisher
{
    private readonly ILogger<MockEventPublisher> _logger = logger;

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        _logger.LogInformation("[MOCK EVENT BUS] Published {EventName}: {Payload}",
            typeof(TEvent).Name, JsonSerializer.Serialize(@event));
        return Task.CompletedTask;
    }
}

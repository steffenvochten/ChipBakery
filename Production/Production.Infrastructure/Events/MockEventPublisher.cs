using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;

namespace Production.Infrastructure.Events;

/// <summary>
/// A temporary implementation of IEventPublisher that logs events as JSON.
/// In a production scenario, this would be replaced with a RabbitMQ or MassTransit implementation.
/// </summary>
public class MockEventPublisher(ILogger<MockEventPublisher> logger) : IEventPublisher
{
    private readonly ILogger<MockEventPublisher> _logger = logger;

    // TODO: Replace with RabbitMQ/MassTransit publisher.
    // 1. Add NuGet: Aspire.RabbitMQ.Client (or MassTransit.RabbitMQ)
    // 2. AppHost: productionService.WithReference(rabbitmq).WaitFor(rabbitmq)
    // 3. Create RabbitMqEventPublisher : IEventPublisher
    // 4. Swap in DependencyInjection.cs
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        _logger.LogInformation("[MOCK EVENT BUS] Published {EventName}: {Payload}",
            typeof(TEvent).Name, JsonSerializer.Serialize(@event));

        return Task.CompletedTask;
    }
}

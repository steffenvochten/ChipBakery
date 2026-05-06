using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;

namespace Warehouse.Infrastructure.Events;

// TODO: Replace with RabbitMQ/MassTransit publisher.
// 1. Add NuGet: Aspire.RabbitMQ.Client (or MassTransit.RabbitMQ)
// 2. AppHost: warehouseService.WithReference(rabbitmq).WaitFor(rabbitmq)
// 3. Create RabbitMqEventPublisher : IEventPublisher
// 4. Swap in DependencyInjection.cs
public class MockEventPublisher(ILogger<MockEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        logger.LogInformation("[MOCK EVENT BUS] Published {EventName}: {Payload}",
            typeof(TEvent).Name, JsonSerializer.Serialize(@event));
        return Task.CompletedTask;
    }
}

using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Loyalty.Infrastructure.Events;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        var eventName = typeof(TEvent).Name;
        var exchangeName = "chipbakery-exchange";
        var routingKey = eventName;

        using var channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            body: body,
            cancellationToken: ct);

        _logger.LogInformation("Published {EventName} to RabbitMQ exchange {ExchangeName}", eventName, exchangeName);
    }
}

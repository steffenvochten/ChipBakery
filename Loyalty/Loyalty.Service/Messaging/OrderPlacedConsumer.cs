using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using Loyalty.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Loyalty.Service.Messaging;

public class OrderPlacedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private string? _queueName;

    private const string ExchangeName = "chipbakery-exchange";

    public OrderPlacedConsumer(
        IServiceProvider serviceProvider,
        ILogger<OrderPlacedConsumer> logger,
        IConnection connection)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        var declareResult = await _channel.QueueDeclareAsync(
            queue: "loyalty-order-placed",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        _queueName = declareResult.QueueName;

        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: ExchangeName,
            routingKey: "OrderPlacedEvent",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var @event = JsonSerializer.Deserialize<OrderPlacedEvent>(message);
                if (@event != null)
                {
                    _logger.LogInformation("Received OrderPlacedEvent for Order {OrderId}", @event.OrderId);

                    if (!Guid.TryParse(@event.CustomerId, out var customerGuid))
                    {
                        _logger.LogWarning(
                            "Skipping points award for Order {OrderId}: CustomerId '{CustomerId}' is not a valid Guid",
                            @event.OrderId, @event.CustomerId);
                    }
                    else
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var loyaltyService = scope.ServiceProvider.GetRequiredService<ILoyaltyService>();

                        var points = @event.Quantity;
                        var description = $"Order placed: {@event.OrderId}";

                        await loyaltyService.AwardPointsAsync(customerGuid, points, description, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPlacedEvent");
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("OrderPlacedConsumer started and listening on {QueueName}", _queueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken: cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}

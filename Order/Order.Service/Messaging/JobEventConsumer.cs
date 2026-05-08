using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using Order.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Order.Service.Messaging;

public class JobEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobEventConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private string? _queueName;

    private const string ExchangeName = "chipbakery-exchange";

    public JobEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<JobEventConsumer> logger,
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
            queue: "order-job-events",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        _queueName = declareResult.QueueName;

        // Bind to multiple events
        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: ExchangeName,
            routingKey: "JobStartedEvent",
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: ExchangeName,
            routingKey: "JobCompletedEvent",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                if (routingKey == "JobStartedEvent")
                {
                    var @event = JsonSerializer.Deserialize<JobStartedEvent>(message);
                    if (@event?.OrderId != null)
                    {
                        _logger.LogInformation("Received JobStartedEvent for Order {@event.OrderId}", @event.OrderId);
                        await orderService.StartOrderProcessingAsync(@event.OrderId.Value, stoppingToken);
                    }
                }
                else if (routingKey == "JobCompletedEvent")
                {
                    var @event = JsonSerializer.Deserialize<JobCompletedEvent>(message);
                    if (@event?.OrderId != null)
                    {
                        _logger.LogInformation("Received JobCompletedEvent for Order {@event.OrderId}", @event.OrderId);
                        await orderService.CompleteOrderAsync(@event.OrderId.Value, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job event {RoutingKey}", routingKey);
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("JobEventConsumer started and listening on {QueueName}", _queueName);

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

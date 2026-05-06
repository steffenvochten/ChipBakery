using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Loyalty.Service.Messaging;

public class JobCompletedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobCompletedConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private string? _queueName;

    private const string ExchangeName = "chipbakery-exchange";

    public JobCompletedConsumer(
        IServiceProvider serviceProvider,
        ILogger<JobCompletedConsumer> logger,
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
            queue: "loyalty-job-completed",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        _queueName = declareResult.QueueName;

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

            try
            {
                var @event = JsonSerializer.Deserialize<JobCompletedEvent>(message);
                if (@event != null)
                {
                    // TODO: JobCompletedEvent does not currently carry a CustomerId, so we
                    // cannot award loyalty points here. Once the shared event is enriched
                    // with CustomerId (or an OrderId we can resolve to a customer), wire up
                    // ILoyaltyService.AwardPointsAsync similar to OrderPlacedConsumer.
                    _logger.LogInformation(
                        "Baking complete for Job {JobId}: Product {ProductId}, Quantity {Quantity} (no points awarded - missing CustomerId)",
                        @event.JobId, @event.ProductId, @event.Quantity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing JobCompletedEvent");
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("JobCompletedConsumer started and listening on {QueueName}", _queueName);

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

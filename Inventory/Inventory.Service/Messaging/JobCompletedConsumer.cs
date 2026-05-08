using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using Inventory.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventory.Service.Messaging;

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
            queue: "inventory-job-completed",
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
                    _logger.LogInformation("Received JobCompletedEvent for Job {JobId}. Restocking Product {ProductId} x{Quantity}", 
                        @event.JobId, @event.ProductId, @event.Quantity);

                    using var scope = _serviceProvider.CreateScope();
                    var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                    await inventoryService.RestockAsync(@event.ProductId, (int)@event.Quantity, stoppingToken);
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

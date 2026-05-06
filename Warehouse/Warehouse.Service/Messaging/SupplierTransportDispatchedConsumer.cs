using System.Text;
using System.Text.Json;
using ChipBakery.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Service.Messaging;

public class SupplierTransportDispatchedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SupplierTransportDispatchedConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private string? _queueName;

    private const string ExchangeName = "chipbakery-exchange";

    public SupplierTransportDispatchedConsumer(
        IServiceProvider serviceProvider,
        ILogger<SupplierTransportDispatchedConsumer> logger,
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
            queue: "warehouse-supplier-dispatched",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        _queueName = declareResult.QueueName;

        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: ExchangeName,
            routingKey: nameof(SupplierTransportDispatchedEvent),
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var @event = JsonSerializer.Deserialize<SupplierTransportDispatchedEvent>(message);
                if (@event != null)
                {
                    _logger.LogInformation(
                        "Received SupplierTransportDispatchedEvent {Id} for {Quantity}{Unit} of {Ingredient}",
                        @event.Id, @event.Quantity, @event.Unit, @event.IngredientName);

                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IWarehouseRepository>();

                    var items = await repo.GetAllAsync(stoppingToken);
                    var existing = items.FirstOrDefault(i =>
                        string.Equals(i.Name, @event.IngredientName, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        var tracked = await repo.GetByIdAsync(existing.Id, stoppingToken);
                        if (tracked != null)
                        {
                            tracked.Quantity += @event.Quantity;
                            repo.Update(tracked);
                        }
                    }
                    else
                    {
                        await repo.AddAsync(new Warehouse.Domain.Entities.WarehouseItem
                        {
                            Id = Guid.NewGuid(),
                            Name = @event.IngredientName,
                            Quantity = @event.Quantity,
                            Unit = @event.Unit
                        }, stoppingToken);
                    }

                    await repo.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SupplierTransportDispatchedEvent");
            }

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "SupplierTransportDispatchedConsumer started and listening on {QueueName}", _queueName);

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

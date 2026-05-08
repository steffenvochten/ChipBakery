using ChipBakery.Shared;
using FluentValidation;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Application.Mapping;
using Order.Domain.Entities;
using Order.Domain.Exceptions;
using Order.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Order.Application.Services;

/// <summary>
/// Core application service for all order operations.
/// Orchestrates repository reads/writes, validates inputs, calls Inventory.Service,
/// and publishes domain events.
/// All business rules live here — not in the entity.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IInventoryClient _inventoryClient;
    private readonly IWarehouseClient _warehouseClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<PlaceOrderRequest> _placeOrderValidator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IInventoryClient inventoryClient,
        IWarehouseClient warehouseClient,
        IEventPublisher eventPublisher,
        IValidator<PlaceOrderRequest> placeOrderValidator,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _inventoryClient = inventoryClient;
        _warehouseClient = warehouseClient;
        _eventPublisher = eventPublisher;
        _placeOrderValidator = placeOrderValidator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        var orders = await _repository.GetAllAsync(ct);
        return orders.ToDtoList();
    }

    /// <inheritdoc/>
    public async Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct)
            ?? throw new OrderNotFoundException(id);

        return order.ToDto();
    }

    /// <inheritdoc/>
    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken ct = default)
    {
        // 1. Validate inputs
        await _placeOrderValidator.ValidateAndThrowAsync(request, ct);

        // 2. Synchronously check ingredients in Warehouse.Service
        var recipeCheck = await _warehouseClient.CheckRecipeAsync(request.ProductId, request.Quantity, ct);
        if (!recipeCheck.Available)
        {
            throw new InvalidOperationException($"Order could not be placed: {recipeCheck.Message ?? "Insufficient ingredients."}");
        }

        // 3. Synchronously check & deduct stock from Inventory.Service.
        //    This is the critical invariant: stock MUST be confirmed before the order is accepted.
        var deductResult = await _inventoryClient.DeductStockAsync(request.ProductId, request.Quantity, ct);

        if (!deductResult.Success)
        {
            // Surface inventory errors as a meaningful exception that the GlobalExceptionHandler maps to 409.
            throw new InvalidOperationException(
                $"Order could not be placed: {deductResult.ErrorMessage ?? "Inventory deduction failed."}");
        }

        // 3. Calculate total price from unit price returned by inventory
        var totalPrice = deductResult.UnitPrice * request.Quantity;

        // 4. Persist the order
        var order = new BakeryOrder
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalPrice = totalPrice,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Placed
        };

        await _repository.AddAsync(order, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order {OrderId} placed for {CustomerName} — product {ProductId} x{Quantity} @ {TotalPrice:C}",
            order.Id, order.CustomerName, order.ProductId, order.Quantity, order.TotalPrice);

        // 5. Publish domain event (currently mock — see MockEventPublisher.cs for RabbitMQ replacement guide)
        await _eventPublisher.PublishAsync(new OrderPlacedEvent(
            order.Id,
            order.CustomerName,
            request.CustomerId,
            order.ProductId,
            order.Quantity,
            order.TotalPrice,
            order.OrderDate), ct);

        return order.ToDto();
    }

    /// <inheritdoc/>
    public async Task<OrderDto> CancelOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct)
            ?? throw new OrderNotFoundException(id);

        if (order.Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException(
                $"Order '{id}' cannot be cancelled because its current status is '{order.Status}'. " +
                "Only orders with status 'Placed' can be cancelled.");
        }

        order.Status = OrderStatus.Cancelled;

        _repository.Update(order);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} cancelled.", order.Id);

        await _eventPublisher.PublishAsync(new OrderCancelledEvent(
            order.Id,
            DateTime.UtcNow), ct);

        return order.ToDto();
    }

    /// <inheritdoc/>
    public async Task StartOrderProcessingAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order == null)
        {
            _logger.LogWarning("Cannot start processing: Order {OrderId} not found", id);
            return;
        }

        if (order.Status != OrderStatus.Placed) return;

        order.Status = OrderStatus.Processing;
        _repository.Update(order);
        await _repository.SaveChangesAsync(ct);
        
        _logger.LogInformation("Order {OrderId} is now Processing.", id);
    }

    /// <inheritdoc/>
    public async Task CompleteOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _repository.GetByIdAsync(id, ct);
        if (order == null)
        {
            _logger.LogWarning("Cannot complete: Order {OrderId} not found", id);
            return;
        }

        if (order.Status == OrderStatus.Completed) return;

        order.Status = OrderStatus.Completed;
        _repository.Update(order);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderId} is now Completed.", id);
    }
}

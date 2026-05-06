using ChipBakery.Shared;
using FluentValidation;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Application.Mapping;
using Inventory.Domain.Entities;
using Inventory.Domain.Events;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Services;

/// <summary>
/// Core application service for all inventory operations.
/// Orchestrates repository reads/writes, validates inputs, and publishes domain events.
/// All business rules (stock validation, etc.) live here — not in the entity.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<CreateInventoryItemRequest> _createValidator;
    private readonly IValidator<UpdateInventoryItemRequest> _updateValidator;
    private readonly IValidator<DeductStockRequest> _deductValidator;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IInventoryRepository repository,
        IEventPublisher eventPublisher,
        IValidator<CreateInventoryItemRequest> createValidator,
        IValidator<UpdateInventoryItemRequest> updateValidator,
        IValidator<DeductStockRequest> deductValidator,
        ILogger<InventoryService> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deductValidator = deductValidator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<InventoryItemDto>> GetAllItemsAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    /// <inheritdoc/>
    public async Task<List<InventoryItemDto>> GetAvailableItemsAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAvailableAsync(ct);
        return items.ToDtoList();
    }

    /// <inheritdoc/>
    public async Task<InventoryItemDto> GetItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new ItemNotFoundException(id);

        return item.ToDto();
    }

    /// <inheritdoc/>
    public async Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, ct);

        var item = new Inventory.Domain.Entities.InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity
        };

        await _repository.AddAsync(item, ct);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Created inventory item {ItemId} ({ItemName})", item.Id, item.Name);

        await _eventPublisher.PublishAsync(new InventoryItemCreatedEvent(
            item.Id,
            item.Name,
            item.Price,
            item.Quantity,
            DateTime.UtcNow), ct);

        return item.ToDto();
    }

    /// <inheritdoc/>
    public async Task<InventoryItemDto> UpdateItemAsync(Guid id, UpdateInventoryItemRequest request, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, ct);

        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new ItemNotFoundException(id);

        // Mutate the tracked entity — EF change tracking will diff these on SaveChanges
        item.Name = request.Name;
        item.Price = request.Price;
        item.Quantity = request.Quantity;

        _repository.Update(item);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Updated inventory item {ItemId} ({ItemName})", item.Id, item.Name);

        return item.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteItemAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new ItemNotFoundException(id);

        _repository.Delete(item);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted inventory item {ItemId} ({ItemName})", item.Id, item.Name);

        await _eventPublisher.PublishAsync(new InventoryItemDeletedEvent(
            item.Id,
            item.Name,
            DateTime.UtcNow), ct);
    }

    /// <inheritdoc/>
    public async Task DeductStockAsync(DeductStockRequest request, CancellationToken ct = default)
    {
        await _deductValidator.ValidateAndThrowAsync(request, ct);

        var item = await _repository.GetByIdAsync(request.ProductId, ct)
            ?? throw new ItemNotFoundException(request.ProductId);

        if (item.Quantity < request.Quantity)
            throw new InsufficientStockException(item.Id, request.Quantity, item.Quantity);

        item.Quantity -= request.Quantity;

        _repository.Update(item);
        await _repository.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deducted {Quantity} units from item {ItemId} ({ItemName}). Remaining: {Remaining}",
            request.Quantity, item.Id, item.Name, item.Quantity);

        await _eventPublisher.PublishAsync(new StockDeductedEvent(
            item.Id,
            item.Name,
            request.Quantity,
            item.Quantity,
            DateTime.UtcNow), ct);

        // Fire an additional event if stock just hit zero
        if (item.Quantity == 0)
        {
            _logger.LogWarning("Stock depleted for item {ItemId} ({ItemName})", item.Id, item.Name);

            await _eventPublisher.PublishAsync(new StockDepletedEvent(
                item.Id,
                item.Name,
                DateTime.UtcNow), ct);
        }
    }
}

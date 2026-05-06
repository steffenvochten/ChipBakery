using FluentValidation;
using Microsoft.Extensions.Logging;
using ChipBakery.Shared;
using Supplier.Application.DTOs;
using Supplier.Application.Interfaces;
using Supplier.Application.Mapping;
using Supplier.Domain.Entities;
using Supplier.Domain.Events;
using Supplier.Domain.Exceptions;
using Supplier.Domain.Interfaces;

namespace Supplier.Application.Services;

public class SupplierService(
    IIngredientSupplyRepository repository,
    IEventPublisher eventPublisher,
    IValidator<CreateIngredientSupplyRequest> createValidator,
    IValidator<UpdateIngredientSupplyRequest> updateValidator,
    ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<IngredientSupplyDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        return item?.ToDto();
    }

    public async Task<List<IngredientSupplyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<IngredientSupplyDto> CreateAsync(CreateIngredientSupplyRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var entity = new IngredientSupply
        {
            Id = Guid.NewGuid(),
            IngredientName = request.IngredientName,
            SupplierName = request.SupplierName,
            Quantity = request.Quantity,
            Price = request.Price,
            ScheduledDate = request.ScheduledDate
        };

        await repository.AddAsync(entity, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Created ingredient supply {Id} for {IngredientName}", entity.Id, entity.IngredientName);

        await eventPublisher.PublishAsync(new IngredientSupplyCreatedEvent(
            entity.Id, entity.IngredientName, entity.SupplierName, entity.Quantity), ct);

        return entity.ToDto();
    }

    public async Task<IngredientSupplyDto> UpdateAsync(Guid id, UpdateIngredientSupplyRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var entity = await repository.GetByIdAsync(id, ct) 
            ?? throw new IngredientSupplyNotFoundException(id);

        entity.IngredientName = request.IngredientName;
        entity.SupplierName = request.SupplierName;
        entity.Quantity = request.Quantity;
        entity.Price = request.Price;
        entity.ScheduledDate = request.ScheduledDate;

        repository.Update(entity);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Updated ingredient supply {Id}", id);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct) 
            ?? throw new IngredientSupplyNotFoundException(id);

        repository.Delete(entity);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Deleted ingredient supply {Id}", id);

        await eventPublisher.PublishAsync(new IngredientSupplyDeletedEvent(id, entity.IngredientName), ct);
    }
}

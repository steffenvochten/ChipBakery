using FluentValidation;
using Microsoft.Extensions.Logging;
using ChipBakery.Shared;
using Supplier.Application.Interfaces;
using Supplier.Application.Mapping;
using Supplier.Domain.Entities;
using Supplier.Domain.Exceptions;
using Supplier.Domain.Interfaces;

namespace Supplier.Application.Services;

public class SupplierService(
    ISupplierTransportRepository repository,
    IIngredientSupplyRepository ingredientRepository,
    IEventPublisher eventPublisher,
    IValidator<Supplier.Application.DTOs.DispatchTransportRequest> validator,
    IValidator<CreateIngredientSupplyRequest> createIngredientValidator,
    IValidator<UpdateIngredientSupplyRequest> updateIngredientValidator,
    IValidator<RestockRequest> restockValidator,
    ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<Supplier.Application.DTOs.SupplierTransportDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        return item?.ToDto();
    }

    public async Task<List<Supplier.Application.DTOs.SupplierTransportDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<Supplier.Application.DTOs.SupplierTransportDto> DispatchTransportAsync(Supplier.Application.DTOs.DispatchTransportRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var entity = await CreateAndPublishTransportAsync(request.IngredientName, request.Quantity, request.Unit, ct);
        return entity.ToDto();
    }

    private async Task<SupplierTransport> CreateAndPublishTransportAsync(string ingredientName, decimal quantity, string unit, CancellationToken ct)
    {
        var entity = new SupplierTransport
        {
            Id = Guid.NewGuid(),
            IngredientName = ingredientName,
            Quantity = quantity,
            Unit = unit,
            Timestamp = DateTime.UtcNow
        };

        await repository.AddAsync(entity, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Dispatched supplier transport {Id} for {IngredientName}", entity.Id, entity.IngredientName);

        await eventPublisher.PublishAsync(new SupplierTransportDispatchedEvent(
            entity.Id,
            entity.IngredientName,
            entity.Quantity,
            entity.Unit,
            entity.Timestamp), ct);

        return entity;
    }

    public async Task<IngredientSupplyDto> CreateIngredientAsync(CreateIngredientSupplyRequest request, CancellationToken ct = default)
    {
        await createIngredientValidator.ValidateAndThrowAsync(request, ct);

        var entity = request.ToEntity();
        await ingredientRepository.AddAsync(entity, ct);
        await ingredientRepository.SaveChangesAsync(ct);

        logger.LogInformation("Created ingredient supply {Id} for {IngredientName}", entity.Id, entity.IngredientName);
        return entity.ToDto();
    }

    public async Task<IngredientSupplyDto> UpdateIngredientAsync(Guid id, UpdateIngredientSupplyRequest request, CancellationToken ct = default)
    {
        await updateIngredientValidator.ValidateAndThrowAsync(request, ct);

        var entity = await ingredientRepository.GetByIdAsync(id, ct)
            ?? throw new IngredientSupplyNotFoundException(id);

        entity.ApplyUpdate(request);
        ingredientRepository.Update(entity);
        await ingredientRepository.SaveChangesAsync(ct);

        return entity.ToDto();
    }

    public async Task DeleteIngredientAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await ingredientRepository.GetByIdAsync(id, ct)
            ?? throw new IngredientSupplyNotFoundException(id);

        ingredientRepository.Remove(entity);
        await ingredientRepository.SaveChangesAsync(ct);
    }

    public async Task<List<IngredientSupplyDto>> ListIngredientsAsync(CancellationToken ct = default)
    {
        var items = await ingredientRepository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<IngredientSupplyDto?> GetIngredientByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await ingredientRepository.GetByIdAsync(id, ct);
        return item?.ToDto();
    }

    public async Task<ChipBakery.Shared.SupplierTransportDto> RestockAsync(RestockRequest request, CancellationToken ct = default)
    {
        await restockValidator.ValidateAndThrowAsync(request, ct);

        var entity = await CreateAndPublishTransportAsync(request.IngredientName, request.Quantity, request.Unit, ct);

        return new ChipBakery.Shared.SupplierTransportDto(
            entity.Id,
            entity.IngredientName,
            entity.Quantity,
            entity.Unit,
            entity.Timestamp);
    }
}

using FluentValidation;
using Microsoft.Extensions.Logging;
using ChipBakery.Shared;
using Supplier.Application.DTOs;
using Supplier.Application.Interfaces;
using Supplier.Application.Mapping;
using Supplier.Domain.Entities;
using Supplier.Domain.Events;
using Supplier.Domain.Interfaces;

namespace Supplier.Application.Services;

public class SupplierService(
    ISupplierTransportRepository repository,
    IEventPublisher eventPublisher,
    IValidator<DispatchTransportRequest> validator,
    ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<SupplierTransportDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        return item?.ToDto();
    }

    public async Task<List<SupplierTransportDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<SupplierTransportDto> DispatchTransportAsync(DispatchTransportRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var entity = new SupplierTransport
        {
            Id = Guid.NewGuid(),
            IngredientName = request.IngredientName,
            Quantity = request.Quantity,
            Unit = request.Unit,
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

        return entity.ToDto();
    }
}

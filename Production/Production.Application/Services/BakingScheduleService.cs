using FluentValidation;
using Microsoft.Extensions.Logging;
using Production.Application.DTOs;
using Production.Application.Interfaces;
using Production.Application.Mapping;
using Production.Domain.Entities;
using Production.Domain.Events;
using Production.Domain.Exceptions;
using Production.Domain.Interfaces;
using ChipBakery.Shared;

namespace Production.Application.Services;

public class BakingScheduleService(
    IBakingScheduleRepository repository,
    IEventPublisher eventPublisher,
    IValidator<CreateBakingScheduleRequest> createValidator,
    ILogger<BakingScheduleService> logger) : IBakingScheduleService
{
    public async Task<BakingScheduleDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item == null) throw new BakingScheduleNotFoundException(id);
        return item.ToDto();
    }

    public async Task<List<BakingScheduleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<BakingScheduleDto> CreateAsync(CreateBakingScheduleRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var schedule = new BakingSchedule
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            ScheduledTime = request.ScheduledTime,
            Status = "Scheduled"
        };

        await repository.AddAsync(schedule, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Baking schedule created for {ProductName} ({Quantity} units)", 
            schedule.ProductName, schedule.Quantity);

        return schedule.ToDto();
    }

    public async Task StartBakingAsync(Guid id, CancellationToken ct = default)
    {
        var schedule = await repository.GetByIdAsync(id, ct);
        if (schedule == null) throw new BakingScheduleNotFoundException(id);

        schedule.Status = "InProgress";
        repository.Update(schedule);
        await repository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new BakingStartedEvent(schedule.Id, schedule.ProductId, schedule.Quantity), ct);
        
        logger.LogInformation("Baking started for {ProductName}", schedule.ProductName);
    }

    public async Task CompleteBakingAsync(Guid id, CancellationToken ct = default)
    {
        var schedule = await repository.GetByIdAsync(id, ct);
        if (schedule == null) throw new BakingScheduleNotFoundException(id);

        schedule.Status = "Completed";
        repository.Update(schedule);
        await repository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new BakingCompletedEvent(schedule.Id, schedule.ProductId, schedule.Quantity), ct);

        logger.LogInformation("Baking completed for {ProductName}", schedule.ProductName);
    }
}

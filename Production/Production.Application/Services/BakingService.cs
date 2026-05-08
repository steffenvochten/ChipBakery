using ChipBakery.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Production.Application.DTOs;
using Production.Application.Interfaces;
using Production.Domain.Entities;
using Production.Domain.Interfaces;

namespace Production.Application.Services;

public class BakingService(
    IBakingJobRepository repository,
    ITrackingService trackingService,
    IWarehouseClient warehouseClient,
    IEventPublisher publisher,
    IValidator<ScheduleBakingJobRequest> scheduleValidator,
    ILogger<BakingService> logger) : IBakingService
{
    public async Task<List<BakingJobDto>> GetAllJobsAsync(CancellationToken ct = default)
    {
        var jobs = await repository.GetAllAsync();
        return jobs
            .Select(j => new BakingJobDto(j.Id, j.ProductId, j.OrderId, j.Quantity, j.Status, j.StartTime, j.EndTime))
            .ToList();
    }

    public async Task<BakingJobDto> ScheduleJobAsync(ScheduleBakingJobRequest request, CancellationToken ct = default)
    {
        await scheduleValidator.ValidateAndThrowAsync(request, ct);

        var job = new Production.Domain.Entities.BakingJob
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            OrderId = request.OrderId,
            Quantity = request.Quantity,
            Status = BakingJobStatus.Scheduled
        };

        await repository.AddAsync(job);
        logger.LogInformation("Scheduled baking job {JobId} for product {ProductId} (Order: {OrderId})", job.Id, request.ProductId, job.OrderId);

        await trackingService.UpdateJobStatusAsync(job.Id, job.Status);

        return new BakingJobDto(job.Id, job.ProductId, job.OrderId, job.Quantity, job.Status, job.StartTime, job.EndTime);
    }

    /// <summary>
    /// Attempts to transition a job from Scheduled or AwaitingIngredients into Baking by atomically
    /// consuming its ingredients via Warehouse.Service. On shortage the job is moved (or left) in
    /// AwaitingIngredients and an <see cref="IngredientShortageEvent"/> is published so agents can react.
    /// Returns true when the job actually transitioned to Baking.
    /// </summary>
    public async Task<bool> TryStartJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await repository.GetByIdAsync(jobId);
        if (job == null)
        {
            logger.LogWarning("Baking job {JobId} not found", jobId);
            return false;
        }

        if (job.Status != BakingJobStatus.Scheduled && job.Status != BakingJobStatus.AwaitingIngredients)
        {
            return false;
        }

        // Quantity is decimal in production but the warehouse recipe is keyed by integer batch counts.
        // Round up so partial batches still consume a full ingredient set.
        var batchCount = (int)Math.Ceiling(job.Quantity);

        var consume = await warehouseClient.ConsumeRecipeAsync(job.ProductId, batchCount, ct);

        if (!consume.Consumed)
        {
            if (job.Status != BakingJobStatus.AwaitingIngredients)
            {
                job.Status = BakingJobStatus.AwaitingIngredients;
                await repository.UpdateAsync(job);
                await trackingService.UpdateJobStatusAsync(job.Id, job.Status);
            }

            logger.LogInformation(
                "Baking job {JobId} awaiting ingredients: {Message}",
                job.Id, consume.Message);

            await publisher.PublishAsync(new IngredientShortageEvent(
                job.Id,
                job.ProductId,
                job.Quantity,
                consume.ShortageIngredientName ?? "unknown",
                consume.ShortageQuantityNeeded ?? 0m,
                consume.ShortageQuantityAvailable ?? 0m,
                consume.ShortageUnit ?? "",
                DateTime.UtcNow), ct);

            return false;
        }

        job.Status = BakingJobStatus.Baking;
        job.StartTime = DateTime.UtcNow;
        await repository.UpdateAsync(job);
        await trackingService.UpdateJobStatusAsync(job.Id, job.Status);

        logger.LogInformation("Started baking job {JobId} for Order {OrderId}", job.Id, job.OrderId);
        
        await publisher.PublishAsync(new JobStartedEvent(
            job.Id,
            job.ProductId,
            job.OrderId,
            job.StartTime.Value), ct);

        return true;
    }

    public async Task StartJobAsync(Guid jobId, CancellationToken ct = default)
    {
        await TryStartJobAsync(jobId, ct);
    }

    public async Task CompleteJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await repository.GetByIdAsync(jobId);
        if (job == null)
        {
            logger.LogWarning("Baking job {JobId} not found", jobId);
            return;
        }

        if (job.Status == BakingJobStatus.Completed)
        {
            return;
        }

        job.Status = BakingJobStatus.Completed;
        job.EndTime = DateTime.UtcNow;
        await repository.UpdateAsync(job);

        logger.LogInformation("Completed baking job {JobId} for Order {OrderId}", jobId, job.OrderId);
        await trackingService.UpdateJobStatusAsync(jobId, job.Status);

        await publisher.PublishAsync(
            new JobCompletedEvent(job.Id, job.ProductId, job.OrderId, job.Quantity, DateTime.UtcNow),
            ct);
    }

    public async Task<List<BakingJobDto>> GetJobsByStatusAsync(string status, CancellationToken ct = default)
    {
        var jobs = await repository.GetByStatusAsync(status);
        return jobs
            .Select(j => new BakingJobDto(j.Id, j.ProductId, j.OrderId, j.Quantity, j.Status, j.StartTime, j.EndTime))
            .ToList();
    }
}

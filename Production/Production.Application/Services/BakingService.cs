using ChipBakery.Shared;
using FluentValidation;
using Production.Application.DTOs;
using Production.Application.Interfaces;
using Production.Domain.Entities;
using Production.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Production.Application.Services;

public class BakingService(
    IBakingJobRepository repository,
    ITrackingService trackingService,
    IEventPublisher publisher,
    IValidator<ScheduleBakingJobRequest> scheduleValidator,
    ILogger<BakingService> logger) : IBakingService
{
    public async Task<List<BakingJobDto>> GetAllJobsAsync(CancellationToken ct = default)
    {
        var jobs = await repository.GetAllAsync();
        return jobs.Select(j => new BakingJobDto(j.Id, j.ProductId, j.Quantity, j.Status, j.StartTime, j.EndTime)).ToList();
    }

    public async Task<BakingJobDto> ScheduleJobAsync(ScheduleBakingJobRequest request, CancellationToken ct = default)
    {
        await scheduleValidator.ValidateAndThrowAsync(request, ct);

        var job = new Production.Domain.Entities.BakingJob
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Status = "Scheduled"
        };

        await repository.AddAsync(job);
        logger.LogInformation("Scheduled baking job {JobId} for product {ProductId}", job.Id, request.ProductId);

        await trackingService.UpdateJobStatusAsync(job.Id, job.Status);

        return new BakingJobDto(job.Id, job.ProductId, job.Quantity, job.Status, job.StartTime, job.EndTime);
    }

    public async Task StartJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await repository.GetByIdAsync(jobId);
        if (job == null)
        {
            logger.LogWarning("Baking job {JobId} not found", jobId);
            return;
        }

        job.Status = "Baking";
        job.StartTime = DateTime.UtcNow;
        await repository.UpdateAsync(job);

        logger.LogInformation("Started baking job {JobId}", jobId);
        await trackingService.UpdateJobStatusAsync(jobId, job.Status);
    }

    public async Task CompleteJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await repository.GetByIdAsync(jobId);
        if (job == null)
        {
            logger.LogWarning("Baking job {JobId} not found", jobId);
            return;
        }

        job.Status = "Completed";
        job.EndTime = DateTime.UtcNow;
        await repository.UpdateAsync(job);

        logger.LogInformation("Completed baking job {JobId}", jobId);
        await trackingService.UpdateJobStatusAsync(jobId, job.Status);

        await publisher.PublishAsync(
            new JobCompletedEvent(job.Id, job.ProductId, job.Quantity, DateTime.UtcNow),
            ct);
    }
}

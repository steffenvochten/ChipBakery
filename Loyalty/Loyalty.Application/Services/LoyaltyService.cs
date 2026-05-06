using FluentValidation;
using Loyalty.Application.DTOs;
using Loyalty.Application.Interfaces;
using Loyalty.Application.Mapping;
using Loyalty.Domain.Entities;
using Loyalty.Domain.Events;
using Loyalty.Domain.Exceptions;
using Loyalty.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ChipBakery.Shared;

namespace Loyalty.Application.Services;

public class LoyaltyService(
    ILoyaltyRepository repository,
    IEventPublisher eventPublisher,
    IValidator<CreateLoyaltyMemberRequest> createValidator,
    IValidator<AddPointsRequest> addPointsValidator,
    IValidator<DeductPointsRequest> deductPointsValidator,
    ILogger<LoyaltyService> logger) : ILoyaltyService
{
    public async Task<List<LoyaltyMemberDto>> GetAllAsync(CancellationToken ct = default)
    {
        var members = await repository.GetAllAsync(ct);
        return members.ToDtoList();
    }

    public async Task<LoyaltyMemberDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var member = await repository.GetByIdAsync(id, ct);
        if (member == null) throw new LoyaltyMemberNotFoundException(id);
        return member.ToDto();
    }

    public async Task<LoyaltyMemberDto> CreateAsync(CreateLoyaltyMemberRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var member = new LoyaltyMember
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            Email = request.Email,
            Points = 0
        };

        await repository.AddAsync(member, ct);
        await repository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new LoyaltyMemberCreatedEvent(member.Id, member.CustomerName, member.Email), ct);

        logger.LogInformation("Loyalty member created: {Id} ({CustomerName})", member.Id, member.CustomerName);
        return member.ToDto();
    }

    public async Task<LoyaltyMemberDto> AddPointsAsync(AddPointsRequest request, CancellationToken ct = default)
    {
        await addPointsValidator.ValidateAndThrowAsync(request, ct);

        var member = await repository.GetByIdAsync(request.Id, ct);
        if (member == null) throw new LoyaltyMemberNotFoundException(request.Id);

        member.Points += request.Points;
        repository.Update(member);
        await repository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new LoyaltyPointsAddedEvent(member.Id, request.Points, member.Points), ct);

        logger.LogInformation("Added {Points} points to member {Id}. New total: {Total}", request.Points, member.Id, member.Points);
        return member.ToDto();
    }

    public async Task<LoyaltyMemberDto> DeductPointsAsync(DeductPointsRequest request, CancellationToken ct = default)
    {
        await deductPointsValidator.ValidateAndThrowAsync(request, ct);

        var member = await repository.GetByIdAsync(request.Id, ct);
        if (member == null) throw new LoyaltyMemberNotFoundException(request.Id);

        if (member.Points < request.Points)
        {
            throw new InsufficientPointsException(member.Id, request.Points, member.Points);
        }

        member.Points -= request.Points;
        repository.Update(member);
        await repository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new LoyaltyPointsDeductedEvent(member.Id, request.Points, member.Points), ct);

        logger.LogInformation("Deducted {Points} points from member {Id}. New total: {Total}", request.Points, member.Id, member.Points);
        return member.ToDto();
    }
}

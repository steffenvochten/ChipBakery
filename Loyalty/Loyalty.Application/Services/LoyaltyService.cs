using Loyalty.Application.DTOs;
using Loyalty.Application.Interfaces;
using Loyalty.Application.Mapping;
using Loyalty.Domain.Entities;
using Loyalty.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ChipBakery.Shared;

namespace Loyalty.Application.Services;

public class LoyaltyService(
    ILoyaltyRepository repository,
    ILogger<LoyaltyService> logger) : ILoyaltyService
{
    public async Task<Loyalty.Application.DTOs.CustomerLoyaltyDto?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var loyalty = await repository.GetByCustomerIdAsync(customerId, ct);
        if (loyalty == null) return null;

        var transactions = await repository.GetTransactionsByCustomerIdAsync(customerId, ct);
        return loyalty.ToDto(transactions);
    }

    public async Task AwardPointsAsync(Guid customerId, int points, string description, CancellationToken ct = default)
    {
        var loyalty = await repository.GetByCustomerIdAsync(customerId, ct);
        
        if (loyalty == null)
        {
            loyalty = new Loyalty.Domain.Entities.CustomerLoyalty { CustomerId = customerId, TotalPoints = 0 };
            loyalty.UpdateTier();
        }

        loyalty.AddPoints(points);
        await repository.AddOrUpdateAsync(loyalty, ct);

        var transaction = new Loyalty.Domain.Entities.LoyaltyTransaction
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Points = points,
            Date = DateTime.UtcNow,
            Description = description
        };
        
        await repository.AddTransactionAsync(transaction, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Awarded {Points} points to customer {CustomerId}. New total: {Total} ({Tier})", 
            points, customerId, loyalty.TotalPoints, loyalty.Tier);
    }
}

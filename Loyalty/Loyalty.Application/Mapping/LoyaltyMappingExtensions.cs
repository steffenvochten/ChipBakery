using Loyalty.Application.DTOs;
using Loyalty.Domain.Entities;

namespace Loyalty.Application.Mapping;

public static class LoyaltyMappingExtensions
{
    public static CustomerLoyaltyDto ToDto(this CustomerLoyalty entity, IEnumerable<LoyaltyTransaction> transactions) =>
        new(
            entity.CustomerId, 
            entity.TotalPoints, 
            entity.Tier.ToString(), 
            transactions.Select(t => t.ToDto()).ToList()
        );

    public static LoyaltyTransactionDto ToDto(this LoyaltyTransaction entity) =>
        new(entity.Id, entity.Points, entity.Date, entity.Description ?? string.Empty);
}

namespace Loyalty.Application.DTOs;

public record CustomerLoyaltyDto(Guid CustomerId, int TotalPoints, string Tier, List<LoyaltyTransactionDto> Transactions);
public record LoyaltyTransactionDto(Guid Id, int Points, DateTime Date, string Description);
public record AwardPointsRequest(Guid CustomerId, int Points, string Description);

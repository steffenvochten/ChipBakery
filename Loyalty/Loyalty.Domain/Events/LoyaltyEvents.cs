namespace Loyalty.Domain.Events;

public record LoyaltyMemberCreatedEvent(Guid Id, string CustomerName, string Email);
public record LoyaltyPointsAddedEvent(Guid Id, int AddedPoints, int NewTotal);
public record LoyaltyPointsDeductedEvent(Guid Id, int DeductedPoints, int NewTotal);

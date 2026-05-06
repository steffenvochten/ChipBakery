namespace Loyalty.Application.DTOs;

public record LoyaltyMemberDto(Guid Id, string CustomerName, int Points, string Email);
public record CreateLoyaltyMemberRequest(string CustomerName, string Email);
public record AddPointsRequest(Guid Id, int Points);
public record DeductPointsRequest(Guid Id, int Points);

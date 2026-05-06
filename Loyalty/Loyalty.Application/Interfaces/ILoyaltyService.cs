using Loyalty.Application.DTOs;

namespace Loyalty.Application.Interfaces;

public interface ILoyaltyService
{
    Task<CustomerLoyaltyDto?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AwardPointsAsync(Guid customerId, int points, string description, CancellationToken ct = default);
}

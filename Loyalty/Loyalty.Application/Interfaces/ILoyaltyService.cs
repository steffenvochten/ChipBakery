using Loyalty.Application.DTOs;

namespace Loyalty.Application.Interfaces;

public interface ILoyaltyService
{
    Task<List<LoyaltyMemberDto>> GetAllAsync(CancellationToken ct = default);
    Task<LoyaltyMemberDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LoyaltyMemberDto> CreateAsync(CreateLoyaltyMemberRequest request, CancellationToken ct = default);
    Task<LoyaltyMemberDto> AddPointsAsync(AddPointsRequest request, CancellationToken ct = default);
    Task<LoyaltyMemberDto> DeductPointsAsync(DeductPointsRequest request, CancellationToken ct = default);
}

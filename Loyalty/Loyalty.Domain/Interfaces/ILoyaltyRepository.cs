using Loyalty.Domain.Entities;

namespace Loyalty.Domain.Interfaces;

public interface ILoyaltyRepository
{
    Task<LoyaltyMember?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LoyaltyMember?> GetByCustomerNameAsync(string customerName, CancellationToken ct = default);
    Task<List<LoyaltyMember>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(LoyaltyMember item, CancellationToken ct = default);
    void Update(LoyaltyMember item);
    void Delete(LoyaltyMember item);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

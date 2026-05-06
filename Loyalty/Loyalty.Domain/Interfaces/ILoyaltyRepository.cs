using Loyalty.Domain.Entities;

namespace Loyalty.Domain.Interfaces;

public interface ILoyaltyRepository
{
    Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddOrUpdateAsync(CustomerLoyalty loyalty, CancellationToken ct = default);
    Task AddTransactionAsync(LoyaltyTransaction transaction, CancellationToken ct = default);
    Task<List<LoyaltyTransaction>> GetTransactionsByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

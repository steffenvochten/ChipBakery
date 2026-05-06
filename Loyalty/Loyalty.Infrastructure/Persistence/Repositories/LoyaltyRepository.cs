using Loyalty.Domain.Entities;
using Loyalty.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Infrastructure.Persistence.Repositories;

public class LoyaltyRepository(LoyaltyDbContext context) : ILoyaltyRepository
{
    public async Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default) =>
        await context.CustomerLoyalties.FindAsync([customerId], ct);

    public async Task AddOrUpdateAsync(CustomerLoyalty loyalty, CancellationToken ct = default)
    {
        var existing = await context.CustomerLoyalties.FindAsync([loyalty.CustomerId], ct);
        if (existing == null)
        {
            await context.CustomerLoyalties.AddAsync(loyalty, ct);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(loyalty);
        }
    }

    public async Task AddTransactionAsync(LoyaltyTransaction transaction, CancellationToken ct = default) =>
        await context.LoyaltyTransactions.AddAsync(transaction, ct);

    public async Task<List<LoyaltyTransaction>> GetTransactionsByCustomerIdAsync(Guid customerId, CancellationToken ct = default) =>
        await context.LoyaltyTransactions
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Date)
            .ToListAsync(ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}

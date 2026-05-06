using Loyalty.Domain.Entities;
using Loyalty.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Infrastructure.Persistence.Repositories;

public class LoyaltyRepository(LoyaltyDbContext context) : ILoyaltyRepository
{
    public async Task<LoyaltyMember?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Members.FindAsync([id], ct);

    public async Task<LoyaltyMember?> GetByCustomerNameAsync(string customerName, CancellationToken ct = default) =>
        await context.Members.FirstOrDefaultAsync(x => x.CustomerName == customerName, ct);

    public async Task<List<LoyaltyMember>> GetAllAsync(CancellationToken ct = default) =>
        await context.Members.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(LoyaltyMember item, CancellationToken ct = default) =>
        await context.Members.AddAsync(item, ct);

    public void Update(LoyaltyMember item) =>
        context.Members.Update(item);

    public void Delete(LoyaltyMember item) =>
        context.Members.Remove(item);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await context.SaveChangesAsync(ct);
}

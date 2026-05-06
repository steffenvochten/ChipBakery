using Microsoft.EntityFrameworkCore;
using Supplier.Domain.Entities;
using Supplier.Domain.Interfaces;

namespace Supplier.Infrastructure.Persistence.Repositories;

public class SupplierTransportRepository(SupplierDbContext context) : ISupplierTransportRepository
{
    public async Task<SupplierTransport?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.SupplierTransports.FindAsync([id], ct);
    }

    public async Task<List<SupplierTransport>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.SupplierTransports
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SupplierTransport item, CancellationToken ct = default)
    {
        await context.SupplierTransports.AddAsync(item, ct);
    }

    public void Update(SupplierTransport item)
    {
        context.SupplierTransports.Update(item);
    }

    public void Delete(SupplierTransport item)
    {
        context.SupplierTransports.Remove(item);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}

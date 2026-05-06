using Supplier.Domain.Entities;

namespace Supplier.Domain.Interfaces;

public interface ISupplierTransportRepository
{
    Task<SupplierTransport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<SupplierTransport>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(SupplierTransport item, CancellationToken ct = default);
    void Update(SupplierTransport item);
    void Delete(SupplierTransport item);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

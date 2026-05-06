using Warehouse.Domain.Entities;

namespace Warehouse.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<WarehouseItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WarehouseItem>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(WarehouseItem item, CancellationToken ct = default);
    void Update(WarehouseItem item);
    void Delete(WarehouseItem item);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

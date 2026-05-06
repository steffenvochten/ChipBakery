using Supplier.Application.DTOs;

namespace Supplier.Application.Interfaces;

public interface ISupplierService
{
    Task<SupplierTransportDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<SupplierTransportDto>> GetAllAsync(CancellationToken ct = default);
    Task<SupplierTransportDto> DispatchTransportAsync(DispatchTransportRequest request, CancellationToken ct = default);
}

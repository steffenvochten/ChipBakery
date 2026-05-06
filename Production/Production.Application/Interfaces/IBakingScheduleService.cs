using Production.Application.DTOs;

namespace Production.Application.Interfaces;

public interface IBakingScheduleService
{
    Task<BakingScheduleDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BakingScheduleDto>> GetAllAsync(CancellationToken ct = default);
    Task<BakingScheduleDto> CreateAsync(CreateBakingScheduleRequest request, CancellationToken ct = default);
    Task StartBakingAsync(Guid id, CancellationToken ct = default);
    Task CompleteBakingAsync(Guid id, CancellationToken ct = default);
}

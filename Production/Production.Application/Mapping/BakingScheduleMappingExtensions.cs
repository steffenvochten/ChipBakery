using Production.Application.DTOs;
using Production.Domain.Entities;

namespace Production.Application.Mapping;

public static class BakingScheduleMappingExtensions
{
    public static BakingScheduleDto ToDto(this BakingSchedule schedule) =>
        new(schedule.Id, schedule.ProductId, schedule.ProductName, schedule.Quantity, schedule.ScheduledTime, schedule.Status);

    public static List<BakingScheduleDto> ToDtoList(this IEnumerable<BakingSchedule> items) =>
        items.Select(i => i.ToDto()).ToList();
}

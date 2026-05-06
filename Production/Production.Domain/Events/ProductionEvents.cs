namespace Production.Domain.Events;

public record BakingStartedEvent(Guid ScheduleId, Guid ProductId, int Quantity);
public record BakingCompletedEvent(Guid ScheduleId, Guid ProductId, int Quantity);

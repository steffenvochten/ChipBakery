namespace Production.Application.DTOs;

public record BakingJobDto(
    Guid Id, 
    Guid ProductId, 
    decimal Quantity, 
    string Status,
    DateTime? StartTime,
    DateTime? EndTime);

public record ScheduleBakingJobRequest(
    Guid ProductId, 
    decimal Quantity);

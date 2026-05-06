namespace Production.Application.DTOs;

public record BakingScheduleDto(
    Guid Id, 
    Guid ProductId, 
    string ProductName, 
    int Quantity, 
    DateTime ScheduledTime, 
    string Status);

public record CreateBakingScheduleRequest(
    Guid ProductId, 
    string ProductName, 
    int Quantity, 
    DateTime ScheduledTime);

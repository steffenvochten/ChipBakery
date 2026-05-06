namespace Production.Domain.Entities;

/// <summary>
/// Represents a scheduled baking task.
/// This is a pure data entity; all business logic lives in the Application layer.
/// </summary>
public class BakingSchedule
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled
}

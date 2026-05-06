namespace Production.Domain.Entities;

public class BakingJob
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, Baking, Completed
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

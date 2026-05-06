namespace Loyalty.Domain.Entities;

public class LoyaltyTransaction
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
}

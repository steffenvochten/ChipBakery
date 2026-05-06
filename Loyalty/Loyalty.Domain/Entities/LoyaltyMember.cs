namespace Loyalty.Domain.Entities;

public class LoyaltyMember
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Email { get; set; } = string.Empty;
}

namespace Loyalty.Domain.Entities;

public enum LoyaltyTier
{
    Bronze,
    Silver,
    Gold
}

public class CustomerLoyalty
{
    public Guid CustomerId { get; set; }
    public int TotalPoints { get; set; }
    public LoyaltyTier Tier { get; set; }

    public void AddPoints(int points)
    {
        TotalPoints += points;
        UpdateTier();
    }

    public void UpdateTier()
    {
        Tier = TotalPoints switch
        {
            < 500 => LoyaltyTier.Bronze,
            < 1000 => LoyaltyTier.Silver,
            _ => LoyaltyTier.Gold
        };
    }
}

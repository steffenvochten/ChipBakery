namespace Warehouse.Domain.Entities;

public class RecipeIngredient
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = string.Empty;
}

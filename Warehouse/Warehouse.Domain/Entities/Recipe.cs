namespace Warehouse.Domain.Entities;

public class Recipe
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<RecipeIngredient> Ingredients { get; set; } = new();
}

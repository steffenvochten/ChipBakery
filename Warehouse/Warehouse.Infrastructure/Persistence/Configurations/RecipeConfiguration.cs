using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId)
            .IsRequired();

        builder.Property(r => r.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ProductId);
    }
}

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IngredientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.QuantityRequired)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(i => i.Unit)
            .IsRequired()
            .HasMaxLength(20);
    }
}

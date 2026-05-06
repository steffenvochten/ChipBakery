using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the EF Core schema mapping and seed data for <see cref="InventoryItem"/>.
/// Keeping this separate from the DbContext keeps both classes focused on a single concern.
/// </summary>
public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(i => i.Quantity)
            .IsRequired();

        // ─── Seed Data ────────────────────────────────────────────────────────────
        // These are the initial products in the bakery catalogue.
        // GUIDs are stable so EF won't re-insert on every startup.
        builder.HasData(
            new InventoryItem
            {
                Id = Guid.Parse("d3b4e7e4-23fb-47df-bc6c-18a0ea12cb24"),
                Name = "Butter Croissant",
                Price = 3.50m,
                Quantity = 100
            },
            new InventoryItem
            {
                Id = Guid.Parse("f2e1a3b4-64df-41ca-87ab-51b1f2e2ac89"),
                Name = "Sourdough Loaf",
                Price = 6.00m,
                Quantity = 50
            },
            new InventoryItem
            {
                Id = Guid.Parse("e8a21d5a-1c7c-47db-93ab-62c1e7f3d91b"),
                Name = "Chocolate Éclair",
                Price = 4.25m,
                Quantity = 25
            }
        );
    }
}

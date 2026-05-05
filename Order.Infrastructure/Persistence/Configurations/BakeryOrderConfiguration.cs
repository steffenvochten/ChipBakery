using ChipBakery.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Persistence.Configurations;

/// <summary>
/// Defines the EF Core schema mapping and seed data for <see cref="BakeryOrder"/>.
/// Keeping this separate from the DbContext keeps both classes focused on a single concern.
/// </summary>
public class BakeryOrderConfiguration : IEntityTypeConfiguration<BakeryOrder>
{
    public void Configure(EntityTypeBuilder<BakeryOrder> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.TotalPrice)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(o => o.Quantity)
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .IsRequired();

        // Store enum as a string for readability in the database.
        // Changing from int to string storage requires a migration if you change this later.
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // ─── Seed Data ────────────────────────────────────────────────────────────────
        // Sample orders for development/demo. GUIDs are stable so EF won't re-insert.
        // Product IDs match the seed data in Inventory.Infrastructure/Persistence/Configurations/InventoryItemConfiguration.cs.
        builder.HasData(
            new BakeryOrder
            {
                Id = Guid.Parse("a1b2c3d4-1111-2222-3333-444455556666"),
                CustomerName = "Alice Baker",
                ProductId = Guid.Parse("d3b4e7e4-23fb-47df-bc6c-18a0ea12cb24"), // Butter Croissant @ 3.50
                Quantity = 2,
                TotalPrice = 7.00m,
                OrderDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                Status = OrderStatus.Completed
            },
            new BakeryOrder
            {
                Id = Guid.Parse("b2c3d4e5-2222-3333-4444-555566667777"),
                CustomerName = "Bob Dough",
                ProductId = Guid.Parse("f2e1a3b4-64df-41ca-87ab-51b1f2e2ac89"), // Sourdough Loaf @ 6.00
                Quantity = 1,
                TotalPrice = 6.00m,
                OrderDate = new DateTime(2026, 2, 20, 14, 00, 0, DateTimeKind.Utc),
                Status = OrderStatus.Placed
            },
            new BakeryOrder
            {
                Id = Guid.Parse("c3d4e5f6-3333-4444-5555-666677778888"),
                CustomerName = "Claire Crust",
                ProductId = Guid.Parse("e8a21d5a-1c7c-47db-93ab-62c1e7f3d91b"), // Chocolate Éclair @ 4.25
                Quantity = 3,
                TotalPrice = 12.75m,
                OrderDate = new DateTime(2026, 3, 10, 09, 15, 0, DateTimeKind.Utc),
                Status = OrderStatus.Cancelled
            }
        );
    }
}

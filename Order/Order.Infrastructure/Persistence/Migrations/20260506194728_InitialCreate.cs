using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Order.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerName", "OrderDate", "ProductId", "Quantity", "Status", "TotalPrice" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-1111-2222-3333-444455556666"), "Alice Baker", new DateTime(2026, 1, 15, 10, 30, 0, 0, DateTimeKind.Utc), new Guid("d3b4e7e4-23fb-47df-bc6c-18a0ea12cb24"), 2, "Completed", 7.00m },
                    { new Guid("b2c3d4e5-2222-3333-4444-555566667777"), "Bob Dough", new DateTime(2026, 2, 20, 14, 0, 0, 0, DateTimeKind.Utc), new Guid("f2e1a3b4-64df-41ca-87ab-51b1f2e2ac89"), 1, "Placed", 6.00m },
                    { new Guid("c3d4e5f6-3333-4444-5555-666677778888"), "Claire Crust", new DateTime(2026, 3, 10, 9, 15, 0, 0, DateTimeKind.Utc), new Guid("e8a21d5a-1c7c-47db-93ab-62c1e7f3d91b"), 3, "Cancelled", 12.75m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}

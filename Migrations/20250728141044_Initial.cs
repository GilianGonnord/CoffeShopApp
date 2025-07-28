using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeShopApp.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coffees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Origin = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RoastLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coffees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ClaimValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Coffees",
                columns: new[] { "Id", "CreatedAt", "Description", "IsAvailable", "Name", "Origin", "Price", "RoastLevel" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 28, 14, 10, 43, 523, DateTimeKind.Utc).AddTicks(390), "Bright and floral with citrus notes", true, "Ethiopian Yirgacheffe", "Ethiopia", 18.99m, "Light" },
                    { 2, new DateTime(2025, 7, 28, 14, 10, 43, 523, DateTimeKind.Utc).AddTicks(931), "Rich and balanced with chocolate undertones", true, "Colombian Supremo", "Colombia", 16.99m, "Medium" },
                    { 3, new DateTime(2025, 7, 28, 14, 10, 43, 523, DateTimeKind.Utc).AddTicks(934), "Full-bodied with earthy and herbal flavors", true, "Sumatra Mandheling", "Indonesia", 19.99m, "Dark" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 28, 14, 10, 43, 372, DateTimeKind.Utc).AddTicks(6343), "admin@coffeeshop.com", "$2a$11$XHGHjvl4v7IRzEKKij5jvuulnTX7dxmmW3PfpJGTeG/Lvdko3pWXi", "admin" },
                    { 2, new DateTime(2025, 7, 28, 14, 10, 43, 521, DateTimeKind.Utc).AddTicks(167), "barista@coffeeshop.com", "$2a$11$dsJkPF6J6Q9YSYnIkwEs.ewhvZvDsFLLdSugraFLszFDlQuoiMHpG", "barista" }
                });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[,]
                {
                    { 1, "IsManager", "true", 1 },
                    { 2, "CanManageCoffee", "true", 1 },
                    { 3, "CanViewCoffee", "true", 1 },
                    { 4, "IsBarista", "true", 2 },
                    { 5, "CanViewCoffee", "true", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Coffees");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

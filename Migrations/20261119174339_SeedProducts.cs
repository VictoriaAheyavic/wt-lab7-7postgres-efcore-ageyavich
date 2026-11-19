using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wt_lab7_7postgres_efcore_ageyavich.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "IsAvailable", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Компьютеры", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Ноутбук", 2500m },
                    { 2, "Аксессуары", new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), true, "Мышь", 80m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}

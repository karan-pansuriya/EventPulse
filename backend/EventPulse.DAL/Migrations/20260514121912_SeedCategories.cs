using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "created_at", "image_path", "is_active", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Concerts & Music" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Sports" },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Tech" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Comedy" },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Movies & Screenings" },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Theatre & Drama" },
                    { 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Workshops" },
                    { 8, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Conferences" },
                    { 9, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Festivals" },
                    { 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Food & Drink" },
                    { 11, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Arts & Exhibitions" },
                    { 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Gaming & Esports" },
                    { 13, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Education" },
                    { 14, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Health & Wellness" },
                    { 15, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Networking" },
                    { 16, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Travel & Adventure" },
                    { 17, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Business" },
                    { 18, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Fashion" },
                    { 19, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Cultural Events" },
                    { 20, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Expos & Trade Shows" },
                    { 21, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Webinars" },
                    { 22, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "/categories/category_image.png", true, "Career & Jobs" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: 22);
        }
    }
}

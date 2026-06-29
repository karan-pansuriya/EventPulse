using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "image_path",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "booking_status",
                table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "image_path",
                table: "categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "booking_status",
                table: "bookings",
                type: "text",
                nullable: false,
                defaultValue: "Confirmed");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 1,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 2,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 3,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 4,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 5,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 6,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 7,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 8,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 9,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 10,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 11,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 12,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 13,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 14,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 15,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 16,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 17,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 18,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 19,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 20,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 21,
                column: "image_path",
                value: "/categories/category_image.png");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 22,
                column: "image_path",
                value: "/categories/category_image.png");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "venues");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "events");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "venues",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 1,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 2,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 3,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 4,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 5,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 6,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 7,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 8,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 9,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 10,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 11,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 12,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 13,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 14,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 15,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 16,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 17,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 18,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 19,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 20,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 21,
                column: "is_active",
                value: true);

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "id",
                keyValue: 22,
                column: "is_active",
                value: true);
        }
    }
}

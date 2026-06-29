using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_name",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name",
                unique: true,
                filter: "\"is_deleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_name",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name",
                unique: true);
        }
    }
}

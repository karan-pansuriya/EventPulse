using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "refresh_tokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "refresh_tokens");
        }
    }
}

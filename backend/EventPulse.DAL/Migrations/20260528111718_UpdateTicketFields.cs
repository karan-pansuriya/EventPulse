using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pdf_path",
                table: "tickets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_code_path",
                table: "tickets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pdf_path",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "qr_code_path",
                table: "tickets");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCheckInFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "venues");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "venues");

            migrationBuilder.DropColumn(
                name: "qr_code",
                table: "tickets");

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "used_at",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_used",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "used_at",
                table: "tickets");

            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "venues",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "venues",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_code",
                table: "tickets",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIdToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "event_id",
                table: "bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_event_id",
                table: "bookings",
                column: "event_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_events_event_id",
                table: "bookings",
                column: "event_id",
                principalTable: "events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_events_event_id",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "ix_bookings_event_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "event_id",
                table: "bookings");
        }
    }
}

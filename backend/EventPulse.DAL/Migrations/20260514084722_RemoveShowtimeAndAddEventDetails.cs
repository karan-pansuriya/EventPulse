using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventPulse.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShowtimeAndAddEventDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_showtimes_showtime_id",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "showtimes");

            migrationBuilder.DropIndex(
                name: "ix_bookings_showtime_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "poster_url",
                table: "events");

            migrationBuilder.DropColumn(
                name: "showtime_id",
                table: "bookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "event_date",
                table: "events",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "events",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "start_time",
                table: "events",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "total_seats",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "event_posters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    poster_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_posters", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_posters_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_posters_event_id",
                table: "event_posters",
                column: "event_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_posters");

            migrationBuilder.DropColumn(
                name: "event_date",
                table: "events");

            migrationBuilder.DropColumn(
                name: "price",
                table: "events");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "events");

            migrationBuilder.DropColumn(
                name: "total_seats",
                table: "events");

            migrationBuilder.AddColumn<string>(
                name: "poster_url",
                table: "events",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "showtime_id",
                table: "bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "showtimes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    venue_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    seats_remaining = table.Column<int>(type: "integer", nullable: false),
                    show_date = table.Column<DateTime>(type: "date", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    total_seats = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_showtimes", x => x.id);
                    table.ForeignKey(
                        name: "fk_showtimes_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_showtimes_venues_venue_id",
                        column: x => x.venue_id,
                        principalTable: "venues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_showtime_id",
                table: "bookings",
                column: "showtime_id");

            migrationBuilder.CreateIndex(
                name: "ix_showtimes_event_id_venue_id_show_date_start_time",
                table: "showtimes",
                columns: new[] { "event_id", "venue_id", "show_date", "start_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_showtimes_venue_id",
                table: "showtimes",
                column: "venue_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_showtimes_showtime_id",
                table: "bookings",
                column: "showtime_id",
                principalTable: "showtimes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

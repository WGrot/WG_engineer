using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtraOptionsForRestaurantSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxAdvanceBookingTime",
                table: "RestaurantSettings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "MaxGuestsPerReservation",
                table: "RestaurantSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MaxReservationDuration",
                table: "RestaurantSettings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MinAdvanceBookingTime",
                table: "RestaurantSettings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "MinGuestsPerReservation",
                table: "RestaurantSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MinReservationDuration",
                table: "RestaurantSettings",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "ReservationsPerUserLimit",
                table: "RestaurantSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAdvanceBookingTime",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "MaxGuestsPerReservation",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "MaxReservationDuration",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "MinAdvanceBookingTime",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "MinGuestsPerReservation",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "MinReservationDuration",
                table: "RestaurantSettings");

            migrationBuilder.DropColumn(
                name: "ReservationsPerUserLimit",
                table: "RestaurantSettings");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addEntititesForGeoCoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Restaurants",
                type: "double precision",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "Restaurants",
                type: "double precision",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructuredAddress_City",
                table: "Restaurants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructuredAddress_Country",
                table: "Restaurants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "Poland");

            migrationBuilder.AddColumn<string>(
                name: "StructuredAddress_PostalCode",
                table: "Restaurants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructuredAddress_Street",
                table: "Restaurants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurant_Location",
                table: "Restaurants",
                columns: new[] { "Location_Latitude", "Location_Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurant_Location",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StructuredAddress_City",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StructuredAddress_Country",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StructuredAddress_PostalCode",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "StructuredAddress_Street",
                table: "Restaurants");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addPostGisSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "LocationPoint",
                table: "Restaurants",
                type: "geography(POINT,4326)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_LocationPoint",
                table: "Restaurants",
                column: "LocationPoint")
                .Annotation("Npgsql:IndexMethod", "GIST");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_LocationPoint",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "LocationPoint",
                table: "Restaurants");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addedPhotosUrlsToRestauration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "photosThumbnailsUrls",
                table: "Restaurants",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "photosUrls",
                table: "Restaurants",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photosThumbnailsUrls",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "photosUrls",
                table: "Restaurants");
        }
    }
}

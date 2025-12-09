using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguredRestaurantImageLinkRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks");

            migrationBuilder.DropColumn(
                name: "photosThumbnailsUrls",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "photosUrls",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "profileThumbnailUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "profileUrl",
                table: "Restaurants");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks");

            migrationBuilder.AddColumn<List<string>>(
                name: "photosThumbnailsUrls",
                table: "Restaurants",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "photosUrls",
                table: "Restaurants",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "profileThumbnailUrl",
                table: "Restaurants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profileUrl",
                table: "Restaurants",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");
        }
    }
}

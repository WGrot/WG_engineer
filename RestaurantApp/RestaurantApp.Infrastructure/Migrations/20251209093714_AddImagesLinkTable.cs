using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesLinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "MenuItems");

            migrationBuilder.AddColumn<int>(
                name: "ImageLinkId",
                table: "MenuItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RestaurantId = table.Column<int>(type: "integer", nullable: true),
                    MenuItemId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_MenuItemId",
                table: "Images",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_RestaurantId",
                table: "Images",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Images_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Images_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ImageLinkId",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "MenuItems",
                type: "text",
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamedImagesLinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Restaurants_RestaurantId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Images_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "ImageLinks");

            migrationBuilder.RenameIndex(
                name: "IX_Images_RestaurantId",
                table: "ImageLinks",
                newName: "IX_ImageLinks_RestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_MenuItemId",
                table: "ImageLinks",
                newName: "IX_ImageLinks_MenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageLinks",
                table: "ImageLinks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_ImageLinks_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                principalTable: "ImageLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageLinks_Restaurants_RestaurantId",
                table: "ImageLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_ImageLinks_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageLinks",
                table: "ImageLinks");

            migrationBuilder.RenameTable(
                name: "ImageLinks",
                newName: "Images");

            migrationBuilder.RenameIndex(
                name: "IX_ImageLinks_RestaurantId",
                table: "Images",
                newName: "IX_Images_RestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageLinks_MenuItemId",
                table: "Images",
                newName: "IX_Images_MenuItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Restaurants_RestaurantId",
                table: "Images",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Images_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguredCascadingDeleteForImageLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_ImageLinks_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_ImageLinkId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_ImageLinks_MenuItemId",
                table: "ImageLinks");

            migrationBuilder.CreateIndex(
                name: "IX_ImageLinks_MenuItemId",
                table: "ImageLinks",
                column: "MenuItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageLinks_MenuItems_MenuItemId",
                table: "ImageLinks",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageLinks_MenuItems_MenuItemId",
                table: "ImageLinks");

            migrationBuilder.DropIndex(
                name: "IX_ImageLinks_MenuItemId",
                table: "ImageLinks");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageLinks_MenuItemId",
                table: "ImageLinks",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_ImageLinks_ImageLinkId",
                table: "MenuItems",
                column: "ImageLinkId",
                principalTable: "ImageLinks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

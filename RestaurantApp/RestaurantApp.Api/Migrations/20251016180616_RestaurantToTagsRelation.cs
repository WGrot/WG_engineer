using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantToTagsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "MenuItemTags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemTags_RestaurantId",
                table: "MenuItemTags",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemTags_Restaurants_RestaurantId",
                table: "MenuItemTags",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemTags_Restaurants_RestaurantId",
                table: "MenuItemTags");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemTags_RestaurantId",
                table: "MenuItemTags");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "MenuItemTags");
        }
    }
}

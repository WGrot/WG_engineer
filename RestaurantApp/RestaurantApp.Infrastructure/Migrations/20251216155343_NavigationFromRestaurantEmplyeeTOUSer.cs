using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NavigationFromRestaurantEmplyeeTOUSer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RestaurantEmployees_UserId",
                table: "RestaurantEmployees",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantEmployees_AspNetUsers_UserId",
                table: "RestaurantEmployees",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantEmployees_AspNetUsers_UserId",
                table: "RestaurantEmployees");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantEmployees_UserId",
                table: "RestaurantEmployees");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addedDescriptionsToItemVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MenuItemVariants",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "MenuItemVariants");
        }
    }
}

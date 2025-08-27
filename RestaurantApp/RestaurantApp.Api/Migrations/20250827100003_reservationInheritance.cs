using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class reservationInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Reservations",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TableId",
                table: "Reservations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_TableId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "Reservations");
        }
    }
}

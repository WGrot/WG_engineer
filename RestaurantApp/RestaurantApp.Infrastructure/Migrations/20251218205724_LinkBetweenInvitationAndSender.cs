using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkBetweenInvitationAndSender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderId",
                table: "EmployeeInvitations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInvitations_SenderId",
                table: "EmployeeInvitations",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeInvitations_AspNetUsers_SenderId",
                table: "EmployeeInvitations",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeInvitations_AspNetUsers_SenderId",
                table: "EmployeeInvitations");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeInvitations_SenderId",
                table: "EmployeeInvitations");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "EmployeeInvitations");
        }
    }
}

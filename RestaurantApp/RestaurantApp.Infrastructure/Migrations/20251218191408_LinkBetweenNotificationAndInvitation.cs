using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkBetweenNotificationAndInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "EmployeeInvitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInvitations_NotificationId",
                table: "EmployeeInvitations",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeInvitations_UserNotifications_NotificationId",
                table: "EmployeeInvitations",
                column: "NotificationId",
                principalTable: "UserNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeInvitations_UserNotifications_NotificationId",
                table: "EmployeeInvitations");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeInvitations_NotificationId",
                table: "EmployeeInvitations");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "EmployeeInvitations");
        }
    }
}

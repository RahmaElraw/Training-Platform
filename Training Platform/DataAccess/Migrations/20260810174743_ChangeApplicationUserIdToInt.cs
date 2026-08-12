using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Training_Platform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeApplicationUserIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserOTPs_AspNetUsers_ApplicationUserId1",
                table: "ApplicationUserOTPs");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserOTPs_ApplicationUserId1",
                table: "ApplicationUserOTPs");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "ApplicationUserOTPs");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicationUserId",
                table: "ApplicationUserOTPs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserOTPs_ApplicationUserId",
                table: "ApplicationUserOTPs",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserOTPs_AspNetUsers_ApplicationUserId",
                table: "ApplicationUserOTPs",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserOTPs_AspNetUsers_ApplicationUserId",
                table: "ApplicationUserOTPs");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserOTPs_ApplicationUserId",
                table: "ApplicationUserOTPs");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "ApplicationUserOTPs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationUserId1",
                table: "ApplicationUserOTPs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserOTPs_ApplicationUserId1",
                table: "ApplicationUserOTPs",
                column: "ApplicationUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserOTPs_AspNetUsers_ApplicationUserId1",
                table: "ApplicationUserOTPs",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

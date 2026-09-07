using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Training_Platform.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateUrl",
                table: "Certificates",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateUrl",
                table: "Certificates");
        }
    }
}

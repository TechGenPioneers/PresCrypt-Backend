using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Admin",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "AdminName",
                table: "Admin",
                newName: "FirstName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Admin",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Admin",
                newName: "AdminName");
        }
    }
}

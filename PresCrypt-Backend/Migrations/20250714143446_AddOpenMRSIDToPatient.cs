using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenMRSIDToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenMrsId",
                table: "Patient",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenMrsId",
                table: "Patient");
        }
    }
}

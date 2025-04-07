using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedChargeForAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Charge",
                table: "Appointments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Charge",
                table: "Appointments");
        }
    }
}

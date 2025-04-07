using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SetNotNullableToHospitalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Hospitals_HospitalId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "HospitalId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Hospitals_HospitalId",
                table: "Appointments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Hospitals_HospitalId",
                table: "Appointments");

            migrationBuilder.AlterColumn<string>(
                name: "HospitalId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Hospitals_HospitalId",
                table: "Appointments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateDoctorAndAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HospitalId",
                table: "Doctor_Availability",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                table: "Doctor_Availability",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Availability_DoctorId",
                table: "Doctor_Availability",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Availability_HospitalId",
                table: "Doctor_Availability",
                column: "HospitalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Availability_Doctors_DoctorId",
                table: "Doctor_Availability",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Availability_Hospitals_HospitalId",
                table: "Doctor_Availability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Doctors_DoctorId",
                table: "Doctor_Availability");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Hospitals_HospitalId",
                table: "Doctor_Availability");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_Availability_DoctorId",
                table: "Doctor_Availability");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_Availability_HospitalId",
                table: "Doctor_Availability");

            migrationBuilder.AlterColumn<string>(
                name: "HospitalId",
                table: "Doctor_Availability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                table: "Doctor_Availability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}

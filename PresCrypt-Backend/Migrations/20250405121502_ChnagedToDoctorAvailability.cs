using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChnagedToDoctorAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Doctor_DoctorId",
                table: "Doctor_Availability");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Hospitals_HospitalId",
                table: "Doctor_Availability");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doctor_Availability",
                table: "Doctor_Availability");

            migrationBuilder.RenameTable(
                name: "Doctor_Availability",
                newName: "DoctorAvailability");

            migrationBuilder.RenameIndex(
                name: "IX_Doctor_Availability_HospitalId",
                table: "DoctorAvailability",
                newName: "IX_DoctorAvailability_HospitalId");

            migrationBuilder.RenameIndex(
                name: "IX_Doctor_Availability_DoctorId",
                table: "DoctorAvailability",
                newName: "IX_DoctorAvailability_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorAvailability",
                table: "DoctorAvailability",
                column: "AvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Doctor_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Hospitals_HospitalId",
                table: "DoctorAvailability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Doctor_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Hospitals_HospitalId",
                table: "DoctorAvailability");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorAvailability",
                table: "DoctorAvailability");

            migrationBuilder.RenameTable(
                name: "DoctorAvailability",
                newName: "Doctor_Availability");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorAvailability_HospitalId",
                table: "Doctor_Availability",
                newName: "IX_Doctor_Availability_HospitalId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "Doctor_Availability",
                newName: "IX_Doctor_Availability_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doctor_Availability",
                table: "Doctor_Availability",
                column: "AvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Availability_Doctor_DoctorId",
                table: "Doctor_Availability",
                column: "DoctorId",
                principalTable: "Doctor",
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
    }
}

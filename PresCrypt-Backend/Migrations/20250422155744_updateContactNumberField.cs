using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateContactNumberField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability");

            migrationBuilder.RenameColumn(
                name: "ContactNo",
                table: "DoctorRequest",
                newName: "ContactNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability");

            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "DoctorRequest",
                newName: "ContactNo");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

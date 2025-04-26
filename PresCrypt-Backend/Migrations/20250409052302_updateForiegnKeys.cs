using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class updateForiegnKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_DoctorRequest_AvailabilityRequestId",
                table: "RequestAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorRequestId",
                table: "RequestAvailability",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAvailability_DoctorRequestId",
                table: "RequestAvailability",
                column: "DoctorRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_DoctorRequest_DoctorRequestId",
                table: "RequestAvailability",
                column: "DoctorRequestId",
                principalTable: "DoctorRequest",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_DoctorRequest_DoctorRequestId",
                table: "RequestAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability");

            migrationBuilder.DropIndex(
                name: "IX_RequestAvailability_DoctorRequestId",
                table: "RequestAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "DoctorRequestId",
                table: "RequestAvailability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_DoctorRequest_AvailabilityRequestId",
                table: "RequestAvailability",
                column: "AvailabilityRequestId",
                principalTable: "DoctorRequest",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestAvailability_Hospitals_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

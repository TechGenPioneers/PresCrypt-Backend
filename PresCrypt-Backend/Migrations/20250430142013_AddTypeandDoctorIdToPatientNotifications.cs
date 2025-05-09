using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeandDoctorIdToPatientNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "PatientNotifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PatientNotifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PatientNotifications_DoctorId",
                table: "PatientNotifications",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientNotifications_Doctor_DoctorId",
                table: "PatientNotifications",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientNotifications_Doctor_DoctorId",
                table: "PatientNotifications");

            migrationBuilder.DropIndex(
                name: "IX_PatientNotifications_DoctorId",
                table: "PatientNotifications");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "PatientNotifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PatientNotifications");
        }
    }
}

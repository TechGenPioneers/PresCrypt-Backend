using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class addNotificationsCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_DoctorRequest_RequestId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_Doctor_DoctorId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_Patient_PatientId",
                table: "AdminNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_DoctorRequest_RequestId",
                table: "AdminNotifications",
                column: "RequestId",
                principalTable: "DoctorRequest",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Doctor_DoctorId",
                table: "AdminNotifications",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Patient_PatientId",
                table: "AdminNotifications",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_DoctorRequest_RequestId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_Doctor_DoctorId",
                table: "AdminNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminNotifications_Patient_PatientId",
                table: "AdminNotifications");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_DoctorRequest_RequestId",
                table: "AdminNotifications",
                column: "RequestId",
                principalTable: "DoctorRequest",
                principalColumn: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Doctor_DoctorId",
                table: "AdminNotifications",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminNotifications_Patient_PatientId",
                table: "AdminNotifications",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "PatientId");
        }
    }
}

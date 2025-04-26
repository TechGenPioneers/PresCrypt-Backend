using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixRequestAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityRequest");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "DoctorRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)");

            migrationBuilder.CreateTable(
                name: "RequestAvailability",
                columns: table => new
                {
                    AvailabilityRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DoctorRequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AvailableEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAvailability", x => x.AvailabilityRequestId);
                    table.ForeignKey(
                        name: "FK_RequestAvailability_DoctorRequest_AvailabilityRequestId",
                        column: x => x.AvailabilityRequestId,
                        principalTable: "DoctorRequest",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestAvailability_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestAvailability_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "DoctorRequest",
                type: "nvarchar(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AvailabilityRequest",
                columns: table => new
                {
                    AvailabilityRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvailableDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AvailableStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    DoctorRequestId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityRequest", x => x.AvailabilityRequestId);
                    table.ForeignKey(
                        name: "FK_AvailabilityRequest_DoctorRequest_AvailabilityRequestId",
                        column: x => x.AvailabilityRequestId,
                        principalTable: "DoctorRequest",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvailabilityRequest_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRequest_HospitalId",
                table: "AvailabilityRequest",
                column: "HospitalId");
        }
    }
}

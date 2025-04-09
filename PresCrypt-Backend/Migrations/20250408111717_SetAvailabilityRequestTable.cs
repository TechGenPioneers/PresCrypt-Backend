using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SetAvailabilityRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hospitals_Doctor_DoctorId",
                table: "Hospitals");

            migrationBuilder.DropTable(
                name: "Doctor_Availability");

            migrationBuilder.DropIndex(
                name: "IX_Hospitals_DoctorId",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Hospitals");

            migrationBuilder.AddColumn<double>(
                name: "Charge",
                table: "Hospitals",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "DoctorAvailability",
                columns: table => new
                {
                    AvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvailableDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvailableStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AvailableEndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailability", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_DoctorAvailability_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorAvailability_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorRequest",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SLMCRegId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SLMCIdImage = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    NIC = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Charge = table.Column<double>(type: "float", nullable: false),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorRequest", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityRequest",
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

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailability_HospitalId",
                table: "DoctorAvailability",
                column: "HospitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvailabilityRequest");

            migrationBuilder.DropTable(
                name: "DoctorAvailability");

            migrationBuilder.DropTable(
                name: "DoctorRequest");

            migrationBuilder.DropColumn(
                name: "Charge",
                table: "Hospitals");

            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "Hospitals",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Doctor_Availability",
                columns: table => new
                {
                    AvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvailableDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AvailableTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_Availability", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_Doctor_Availability_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hospitals_DoctorId",
                table: "Hospitals",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Availability_DoctorId",
                table: "Doctor_Availability",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hospitals_Doctor_DoctorId",
                table: "Hospitals",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

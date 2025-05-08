using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    public partial class AfterMerge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the Admin table
            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdminName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.AdminId);
                });

            // Create the DoctorRequest table
            migrationBuilder.CreateTable(
                name: "DoctorRequest",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SLMCRegId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SLMCIdImage = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    NIC = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Charge = table.Column<double>(type: "float", nullable: false),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorRequest", x => x.RequestId);
                });

            // Create the RequestAvailability table with foreign keys
            migrationBuilder.CreateTable(
                name: "RequestAvailability",
                columns: table => new
                {
                    AvailabilityRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DoctorRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AvailableDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableStartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    AvailableEndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestAvailability", x => x.AvailabilityRequestId);
                    table.ForeignKey(
                        name: "FK_RequestAvailability_DoctorRequest_DoctorRequestId",
                        column: x => x.DoctorRequestId,
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

            // Add any necessary indexes
            migrationBuilder.CreateIndex(
                name: "IX_RequestAvailability_DoctorRequestId",
                table: "RequestAvailability",
                column: "DoctorRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestAvailability_HospitalId",
                table: "RequestAvailability",
                column: "HospitalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the newly created tables
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "DoctorRequest");

            migrationBuilder.DropTable(
                name: "RequestAvailability");
        }
    }
}

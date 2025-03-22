using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDoctorAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the line adding the HospitalId column
            // migrationBuilder.AddColumn<string>(
            //    name: "HospitalId",
            //    table: "Doctor_Availability",
            //    type: "nvarchar(450)",
            //    nullable: false,
            //    defaultValue: "");

            // Create the index on Doctor_Availability table if needed, can be removed if already exists
            migrationBuilder.CreateIndex(
                name: "IX_Doctor_Availability_HospitalId",
                table: "Doctor_Availability",
                column: "HospitalId");

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
                name: "FK_Doctor_Availability_Hospitals_HospitalId",
                table: "Doctor_Availability");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_Availability_HospitalId",
                table: "Doctor_Availability");

            // Remove the column from Doctor_Availability table if necessary
            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "Doctor_Availability");

            // Creating the HospitalDoctor table again (if needed)
            migrationBuilder.CreateTable(
                name: "HospitalDoctor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalDoctor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HospitalDoctor_Doctor_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctor",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HospitalDoctor_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "HospitalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HospitalDoctor_DoctorId",
                table: "HospitalDoctor",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HospitalDoctor_HospitalId",
                table: "HospitalDoctor",
                column: "HospitalId");
        }
    }
}

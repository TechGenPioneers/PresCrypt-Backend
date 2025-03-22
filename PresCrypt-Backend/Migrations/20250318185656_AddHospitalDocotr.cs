using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalDocotr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hospitals_Doctor_DoctorId",
                table: "Hospitals");

            migrationBuilder.DropIndex(
                name: "IX_Hospitals_DoctorId",
                table: "Hospitals");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Hospitals");

            migrationBuilder.CreateTable(
                name: "HospitalDoctor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HospitalDoctor");

            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "Hospitals",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Hospitals_DoctorId",
                table: "Hospitals",
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

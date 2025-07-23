using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleUserIDToContactUs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientContactUs_Patient_PatientId",
                table: "PatientContactUs");

            migrationBuilder.DropIndex(
                name: "IX_PatientContactUs_PatientId",
                table: "PatientContactUs");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "PatientContactUs");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "PatientContactUs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PatientContactUs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "PatientContactUs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PatientContactUs");

            migrationBuilder.AddColumn<string>(
                name: "PatientId",
                table: "PatientContactUs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PatientContactUs_PatientId",
                table: "PatientContactUs",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientContactUs_Patient_PatientId",
                table: "PatientContactUs",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

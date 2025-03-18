using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNamesnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Doctor_UserId",
                table: "Doctor_Availability");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Doctor_Availability",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_Doctor_Availability_UserId",
                table: "Doctor_Availability",
                newName: "IX_Doctor_Availability_DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Availability_Doctor_DoctorId",
                table: "Doctor_Availability",
                column: "DoctorId",
                principalTable: "Doctor",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctor_Availability_Doctor_DoctorId",
                table: "Doctor_Availability");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Doctor_Availability",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Doctor_Availability_DoctorId",
                table: "Doctor_Availability",
                newName: "IX_Doctor_Availability_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctor_Availability_Doctor_UserId",
                table: "Doctor_Availability",
                column: "UserId",
                principalTable: "Doctor",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

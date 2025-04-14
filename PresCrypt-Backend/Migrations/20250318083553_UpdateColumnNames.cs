using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Patient",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Doctor",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Admin",
                newName: "AdminId");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Id",
                table: "Doctor",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Patient",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Doctor",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "Admin",
                newName: "UserId");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Id",
                table: "Doctor",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");
        }
    }
}

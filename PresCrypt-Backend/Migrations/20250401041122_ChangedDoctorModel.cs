using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDoctorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Doctor",
                newName: "SLMCIdImage");

            migrationBuilder.RenameColumn(
                name: "DoctorName",
                table: "Doctor",
                newName: "LastName");

            migrationBuilder.AlterColumn<string>(
                name: "NIC",
                table: "Doctor",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<byte[]>(
                name: "DoctorImage",
                table: "Doctor",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Doctor",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorImage",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Doctor");

            migrationBuilder.RenameColumn(
                name: "SLMCIdImage",
                table: "Doctor",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Doctor",
                newName: "DoctorName");

            migrationBuilder.AlterColumn<long>(
                name: "NIC",
                table: "Doctor",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

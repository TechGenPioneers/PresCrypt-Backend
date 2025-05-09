using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePatientDoctorModels : Migration
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

            migrationBuilder.RenameColumn(
                name: "ConfirmPassword",
                table: "Doctor",
                newName: "ContactNumber");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Patient");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Patient",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropColumn(
               name: "Status",
               table: "Doctor");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Doctor",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Doctor",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Doctor");

            migrationBuilder.RenameColumn(
                name: "SLMCIdImage",
                table: "Doctor",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Doctor",
                newName: "DoctorName");

            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "Doctor",
                newName: "ConfirmPassword");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Patient",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Doctor",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}

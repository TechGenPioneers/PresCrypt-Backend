using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class LatestFetch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "Doctor");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Patient",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Patient");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "Doctor",
                type: "datetime2",
                nullable: true);
        }
    }
}

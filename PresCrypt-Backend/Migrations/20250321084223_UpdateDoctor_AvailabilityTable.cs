using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctor_AvailabilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableDate",
                table: "Doctor_Availability");

            migrationBuilder.AddColumn<string>(
                name: "AvailableDay",
                table: "Doctor_Availability",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableDay",
                table: "Doctor_Availability");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableDate",
                table: "Doctor_Availability",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}

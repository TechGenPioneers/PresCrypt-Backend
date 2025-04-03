using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedAvalableDaysStartEndTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableDate",
                table: "Doctor_Availability");

            migrationBuilder.RenameColumn(
                name: "AvailableTime",
                table: "Doctor_Availability",
                newName: "AvailableStartTime");

            migrationBuilder.AddColumn<string>(
                name: "AvailableDay",
                table: "Doctor_Availability",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AvailableEndTime",
                table: "Doctor_Availability",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableDay",
                table: "Doctor_Availability");

            migrationBuilder.DropColumn(
                name: "AvailableEndTime",
                table: "Doctor_Availability");

            migrationBuilder.RenameColumn(
                name: "AvailableStartTime",
                table: "Doctor_Availability",
                newName: "AvailableTime");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableDate",
                table: "Doctor_Availability",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresCrypt_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RmCancelCOutnAndAddLastCancelledDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelCount",
                table: "Patient");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCancelledDate",
                table: "Patient",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCancelledDate",
                table: "Patient");

            migrationBuilder.AddColumn<int>(
                name: "CancelCount",
                table: "Patient",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

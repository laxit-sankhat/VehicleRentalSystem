using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleRentalSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDLExpiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DLExpiryDate",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DLExpiryDate",
                table: "Users");
        }
    }
}

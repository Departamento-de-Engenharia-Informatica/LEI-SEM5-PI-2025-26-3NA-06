using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjArqsi.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmationTokenExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationTokenExpiry",
                table: "Users");
        }
    }
}

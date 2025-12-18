using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjArqsi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVesselIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Vessels",
                table: "Vessels");

            // Alter column type
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Vessels",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Recreate primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Vessels",
                table: "Vessels",
                column: "Id");

            // Add IMO column
            migrationBuilder.AddColumn<string>(
                name: "IMO",
                table: "Vessels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop IMO column
            migrationBuilder.DropColumn(
                name: "IMO",
                table: "Vessels");

            // Drop primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Vessels",
                table: "Vessels");

            // Alter column type back to string
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Vessels",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Recreate primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Vessels",
                table: "Vessels",
                column: "Id");
        }
    }
}

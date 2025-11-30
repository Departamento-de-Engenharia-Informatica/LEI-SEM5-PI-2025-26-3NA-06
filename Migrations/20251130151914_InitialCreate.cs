using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjArqsi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VesselTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeName_Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeDescription_Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeCapacity_Value = table.Column<int>(type: "int", nullable: false),
                    MaxRows_Value = table.Column<int>(type: "int", nullable: false),
                    MaxBays_Value = table.Column<int>(type: "int", nullable: false),
                    MaxTiers_Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselTypes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VesselTypes");
        }
    }
}

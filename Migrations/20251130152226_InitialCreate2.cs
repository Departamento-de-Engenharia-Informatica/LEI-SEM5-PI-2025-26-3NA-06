using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjArqsi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeName_Value",
                table: "VesselTypes",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "TypeDescription_Value",
                table: "VesselTypes",
                newName: "TypeDescription");

            migrationBuilder.RenameColumn(
                name: "TypeCapacity_Value",
                table: "VesselTypes",
                newName: "TypeCapacity");

            migrationBuilder.RenameColumn(
                name: "MaxTiers_Value",
                table: "VesselTypes",
                newName: "MaxTiers");

            migrationBuilder.RenameColumn(
                name: "MaxRows_Value",
                table: "VesselTypes",
                newName: "MaxRows");

            migrationBuilder.RenameColumn(
                name: "MaxBays_Value",
                table: "VesselTypes",
                newName: "MaxBays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "VesselTypes",
                newName: "TypeName_Value");

            migrationBuilder.RenameColumn(
                name: "TypeDescription",
                table: "VesselTypes",
                newName: "TypeDescription_Value");

            migrationBuilder.RenameColumn(
                name: "TypeCapacity",
                table: "VesselTypes",
                newName: "TypeCapacity_Value");

            migrationBuilder.RenameColumn(
                name: "MaxTiers",
                table: "VesselTypes",
                newName: "MaxTiers_Value");

            migrationBuilder.RenameColumn(
                name: "MaxRows",
                table: "VesselTypes",
                newName: "MaxRows_Value");

            migrationBuilder.RenameColumn(
                name: "MaxBays",
                table: "VesselTypes",
                newName: "MaxBays_Value");
        }
    }
}

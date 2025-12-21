using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjArqsi.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoManifestsToVVN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferredVesselId",
                table: "VesselVisitNotifications",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CargoManifests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManifestType = table.Column<int>(type: "int", nullable: false),
                    VesselVisitNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoManifests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoManifests_VesselVisitNotifications_VesselVisitNotificationId",
                        column: x => x.VesselVisitNotificationId,
                        principalTable: "VesselVisitNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManifestEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceStorageAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetStorageAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CargoManifestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManifestEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManifestEntries_CargoManifests_CargoManifestId",
                        column: x => x.CargoManifestId,
                        principalTable: "CargoManifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VVN_Status",
                table: "VesselVisitNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VVN_VesselId",
                table: "VesselVisitNotifications",
                column: "ReferredVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoManifests_VesselVisitNotificationId",
                table: "CargoManifests",
                column: "VesselVisitNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ManifestEntries_CargoManifestId",
                table: "ManifestEntries",
                column: "CargoManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_ManifestEntries_ContainerId",
                table: "ManifestEntries",
                column: "ContainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManifestEntries");

            migrationBuilder.DropTable(
                name: "CargoManifests");

            migrationBuilder.DropIndex(
                name: "IX_VVN_Status",
                table: "VesselVisitNotifications");

            migrationBuilder.DropIndex(
                name: "IX_VVN_VesselId",
                table: "VesselVisitNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "ReferredVesselId",
                table: "VesselVisitNotifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}

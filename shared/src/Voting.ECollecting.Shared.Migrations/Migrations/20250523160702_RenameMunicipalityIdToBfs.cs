using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMunicipalityIdToBfs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MunicipalityId",
                table: "InitiativeSubTypes",
                newName: "Bfs");

            migrationBuilder.RenameColumn(
                name: "MunicipalityId",
                table: "Decrees",
                newName: "Bfs");

            migrationBuilder.RenameColumn(
                name: "MunicipalityId",
                table: "Collections",
                newName: "Bfs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Bfs",
                table: "InitiativeSubTypes",
                newName: "MunicipalityId");

            migrationBuilder.RenameColumn(
                name: "Bfs",
                table: "Decrees",
                newName: "MunicipalityId");

            migrationBuilder.RenameColumn(
                name: "Bfs",
                table: "Collections",
                newName: "MunicipalityId");
        }
    }
}

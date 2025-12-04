using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeResidenceToBfs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Residence",
                table: "InitiativeCommitteeMembers",
                newName: "PoliticalBfs");

            migrationBuilder.RenameColumn(
                name: "PoliticalResidence",
                table: "InitiativeCommitteeMembers",
                newName: "Bfs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoliticalBfs",
                table: "InitiativeCommitteeMembers",
                newName: "Residence");

            migrationBuilder.RenameColumn(
                name: "Bfs",
                table: "InitiativeCommitteeMembers",
                newName: "PoliticalResidence");
        }
    }
}

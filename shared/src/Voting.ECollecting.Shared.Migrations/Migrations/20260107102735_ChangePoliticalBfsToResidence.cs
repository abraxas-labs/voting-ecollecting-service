using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangePoliticalBfsToResidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoliticalBfs",
                table: "InitiativeCommitteeMembers",
                newName: "PoliticalResidence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoliticalResidence",
                table: "InitiativeCommitteeMembers",
                newName: "PoliticalBfs");
        }
    }
}

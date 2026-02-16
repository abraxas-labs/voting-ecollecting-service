using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressToCommitteeMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "InitiativeCommitteeMembers",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "InitiativeCommitteeMembers",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "InitiativeCommitteeMembers",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "InitiativeCommitteeMembers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class CommitteeMemberSortIndexNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SortIndex",
                table: "InitiativeCommitteeMembers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SortIndex",
                table: "InitiativeCommitteeMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

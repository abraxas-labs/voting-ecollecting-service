using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeCommitteeList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommitteeListOfInitiativeId",
                table: "Files",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_CommitteeListOfInitiativeId",
                table: "Files",
                column: "CommitteeListOfInitiativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Collections_CommitteeListOfInitiativeId",
                table: "Files",
                column: "CommitteeListOfInitiativeId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Collections_CommitteeListOfInitiativeId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_CommitteeListOfInitiativeId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CommitteeListOfInitiativeId",
                table: "Files");
        }
    }
}

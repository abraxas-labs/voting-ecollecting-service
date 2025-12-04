using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDecreeCitizenLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DecreeId",
                table: "CollectionCitizenLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogs_DecreeId_VotingStimmregisterIdMac",
                table: "CollectionCitizenLogs",
                columns: new[] { "DecreeId", "VotingStimmregisterIdMac" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeId",
                table: "CollectionCitizenLogs",
                column: "DecreeId",
                principalTable: "Decrees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeId",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizenLogs_DecreeId_VotingStimmregisterIdMac",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropColumn(
                name: "DecreeId",
                table: "CollectionCitizenLogs");
        }
    }
}

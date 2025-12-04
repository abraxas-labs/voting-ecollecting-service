using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeCommitteeMemberToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IamUserId",
                table: "InitiativeCommitteeMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignatureFileId",
                table: "InitiativeCommitteeMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "InitiativeCommitteeMembers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiry",
                table: "InitiativeCommitteeMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitiativeCommitteeMembers_SignatureFileId",
                table: "InitiativeCommitteeMembers",
                column: "SignatureFileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InitiativeCommitteeMembers_Files_SignatureFileId",
                table: "InitiativeCommitteeMembers",
                column: "SignatureFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InitiativeCommitteeMembers_Files_SignatureFileId",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropIndex(
                name: "IX_InitiativeCommitteeMembers_SignatureFileId",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "IamUserId",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "SignatureFileId",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "InitiativeCommitteeMembers");

            migrationBuilder.DropColumn(
                name: "TokenExpiry",
                table: "InitiativeCommitteeMembers");
        }
    }
}

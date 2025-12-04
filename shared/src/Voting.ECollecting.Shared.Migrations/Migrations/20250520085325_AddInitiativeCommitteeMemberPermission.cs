using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeCommitteeMemberPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InitiativeCommitteeMemberId",
                table: "CollectionPermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_InitiativeCommitteeMemberId",
                table: "CollectionPermissions",
                column: "InitiativeCommitteeMemberId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionPermissions_InitiativeCommitteeMembers_Initiative~",
                table: "CollectionPermissions",
                column: "InitiativeCommitteeMemberId",
                principalTable: "InitiativeCommitteeMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- citizen service
                                     GRANT UPDATE ON "CollectionPermissions" TO citizenservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionPermissions_InitiativeCommitteeMembers_Initiative~",
                table: "CollectionPermissions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_InitiativeCommitteeMemberId",
                table: "CollectionPermissions");

            migrationBuilder.DropColumn(
                name: "InitiativeCommitteeMemberId",
                table: "CollectionPermissions");
        }
    }
}

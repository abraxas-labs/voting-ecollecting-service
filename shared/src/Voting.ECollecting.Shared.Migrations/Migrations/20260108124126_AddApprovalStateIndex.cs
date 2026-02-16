using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalStateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InitiativeCommitteeMembers_ApprovalState",
                table: "InitiativeCommitteeMembers",
                column: "ApprovalState");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_State",
                table: "CollectionPermissions",
                column: "State");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT UPDATE ON "CollectionPermissions" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => throw new InvalidOperationException("Downgrade not supported");
    }
}

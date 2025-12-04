using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionPermissionUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                                WITH duplicates AS (
                                    SELECT ""Id"",
                                    ROW_NUMBER() OVER (PARTITION BY ""Email"", ""CollectionId"" ORDER BY ""Id"") AS rn
                                FROM ""CollectionPermissions"")
                                DELETE FROM ""CollectionPermissions"" WHERE ""Id"" IN (
                                    SELECT ""Id"" FROM duplicates WHERE rn > 1
                                );
                                ");

            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_CollectionId",
                table: "CollectionPermissions");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_CollectionId_Email",
                table: "CollectionPermissions",
                columns: new[] { "CollectionId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_CollectionId_Email",
                table: "CollectionPermissions");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_CollectionId",
                table: "CollectionPermissions",
                column: "CollectionId");
        }
    }
}

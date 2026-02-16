using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCollectionPermissionIndicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_CollectionId_Email",
                table: "CollectionPermissions");

            // Appends "-copy-{Id}" to duplicates to ensure uniqueness
            // this will result in invalid user ids on some environments,
            // but this is acceptable as these environments will be rebuilt before production release.
            migrationBuilder.Sql(
                """
                 UPDATE "CollectionPermissions"
                 SET "IamUserId" = "IamUserId" || '-copy-' || "Id"
                 WHERE "Id" IN (
                     SELECT "Id"
                     FROM (
                         SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "CollectionId", "IamUserId" ORDER BY "Id") as rn
                         FROM "CollectionPermissions"
                         WHERE "IamUserId" <> ''
                     ) t
                     WHERE t.rn > 1
                 );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_CollectionId_IamUserId",
                table: "CollectionPermissions",
                columns: new[] { "CollectionId", "IamUserId" },
                unique: true,
                filter: "\"IamUserId\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_Owner",
                table: "CollectionPermissions",
                column: "CollectionId",
                unique: true,
                filter: "\"Role\" = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_CollectionId_IamUserId",
                table: "CollectionPermissions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_Owner",
                table: "CollectionPermissions");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_CollectionId_Email",
                table: "CollectionPermissions",
                columns: new[] { "CollectionId", "Email" },
                unique: true);
        }
    }
}

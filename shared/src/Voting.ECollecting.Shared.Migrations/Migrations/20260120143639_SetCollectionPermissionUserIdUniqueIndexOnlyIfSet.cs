using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetCollectionPermissionUserIdUniqueIndexOnlyIfSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionPermissions_CollectionId_IamUserId",
                table: "CollectionPermissions");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionPermissions_CollectionId_IamUserId",
                table: "CollectionPermissions",
                columns: new[] { "CollectionId", "IamUserId" },
                unique: true,
                filter: "\"IamUserId\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException("Not supported");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionTokenAndState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accepted",
                table: "CollectionPermissions");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "CollectionPermissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "CollectionPermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiry",
                table: "CollectionPermissions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "CollectionPermissions");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "CollectionPermissions");

            migrationBuilder.DropColumn(
                name: "TokenExpiry",
                table: "CollectionPermissions");

            migrationBuilder.AddColumn<bool>(
                name: "Accepted",
                table: "CollectionPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitiativeLockedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LockedFields_Address",
                table: "Collections",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockedFields_Description",
                table: "Collections",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockedFields_Wording",
                table: "Collections",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedFields_Address",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "LockedFields_Description",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "LockedFields_Wording",
                table: "Collections");
        }
    }
}

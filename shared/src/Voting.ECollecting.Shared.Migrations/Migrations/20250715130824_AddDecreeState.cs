using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDecreeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CameNotAboutReason",
                table: "Decrees",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Decrees",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameNotAboutReason",
                table: "Decrees");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Decrees");
        }
    }
}

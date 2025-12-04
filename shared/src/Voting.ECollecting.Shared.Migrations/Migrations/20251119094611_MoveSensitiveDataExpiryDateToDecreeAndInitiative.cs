using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveSensitiveDataExpiryDateToDecreeAndInitiative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateBag_DecreeId",
                table: "UserNotifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateBag_DecreeName",
                table: "UserNotifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SensitiveDataExpiryDate",
                table: "Decrees",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateBag_DecreeId",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "TemplateBag_DecreeName",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "SensitiveDataExpiryDate",
                table: "Decrees");
        }
    }
}

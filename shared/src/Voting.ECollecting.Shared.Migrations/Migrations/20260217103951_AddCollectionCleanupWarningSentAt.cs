using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionCleanupWarningSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "TemplateBag_CollectionCleanupDate",
                table: "UserNotifications",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CleanupWarningSentAt",
                table: "Collections",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateBag_CollectionCleanupDate",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "CleanupWarningSentAt",
                table: "Collections");
        }
    }
}

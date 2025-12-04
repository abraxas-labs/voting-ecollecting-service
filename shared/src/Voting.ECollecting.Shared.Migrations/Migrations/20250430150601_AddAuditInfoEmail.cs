using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditInfoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "ImportStatistics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "ImportStatistics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "Decrees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "Decrees",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "Collections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "Collections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "CollectionMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "CollectionMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "CollectionCounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "CollectionCounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "AccessControlListDois",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "AccessControlListDois",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "ImportStatistics");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "ImportStatistics");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "Decrees");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "Decrees");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "CollectionMessages");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "CollectionMessages");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "CollectionCounts");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "CollectionCounts");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "AccessControlListDois");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "AccessControlListDois");
        }
    }
}

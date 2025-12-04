using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFileEntityAudited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AuditInfo_CreatedAt",
                table: "Files",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByEmail",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedById",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_CreatedByName",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AuditInfo_ModifiedAt",
                table: "Files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByEmail",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedById",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditInfo_ModifiedByName",
                table: "Files",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByEmail",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedById",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_CreatedByName",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedAt",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByEmail",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedById",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "AuditInfo_ModifiedByName",
                table: "Files");
        }
    }
}

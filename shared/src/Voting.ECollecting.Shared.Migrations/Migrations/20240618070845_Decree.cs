using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class Decree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecreeStateId",
                table: "Decrees");

            migrationBuilder.RenameColumn(
                name: "FederalLevelId",
                table: "Decrees",
                newName: "DomainOfInfluenceType");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_ModifiedByName",
                table: "Decrees",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_ModifiedById",
                table: "Decrees",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DomainOfInfluenceType",
                table: "Decrees",
                newName: "FederalLevelId");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_ModifiedByName",
                table: "Decrees",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditInfo_ModifiedById",
                table: "Decrees",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecreeStateId",
                table: "Decrees",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

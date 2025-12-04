using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAclSignatureProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ECollectingMaxElectronicSignaturePercent",
                table: "AccessControlListDois",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ECollectingMinSignatureCount",
                table: "AccessControlListDois",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ECollectingMaxElectronicSignaturePercent",
                table: "AccessControlListDois");

            migrationBuilder.DropColumn(
                name: "ECollectingMinSignatureCount",
                table: "AccessControlListDois");
        }
    }
}

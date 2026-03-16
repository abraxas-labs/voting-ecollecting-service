using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoiAddressName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ECollectingNameForProtocol",
                table: "DomainOfInfluences");

            migrationBuilder.RenameColumn(
                name: "BasisNameForProtocol",
                table: "DomainOfInfluences",
                newName: "AddressName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressName",
                table: "DomainOfInfluences",
                newName: "BasisNameForProtocol");

            migrationBuilder.AddColumn<string>(
                name: "ECollectingNameForProtocol",
                table: "DomainOfInfluences",
                type: "text",
                nullable: true);
        }
    }
}

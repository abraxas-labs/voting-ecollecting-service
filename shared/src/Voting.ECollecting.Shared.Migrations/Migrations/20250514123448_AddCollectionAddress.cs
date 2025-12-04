using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCommittee",
                table: "Collections");

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Address_CommitteeOrPerson",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Address_Locality",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<string>(
                name: "Address_StreetOrPostOfficeBox",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_CommitteeOrPerson",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Address_Locality",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Address_StreetOrPostOfficeBox",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Collections");

            migrationBuilder.AddColumn<string>(
                name: "AddressCommittee",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);
        }
    }
}

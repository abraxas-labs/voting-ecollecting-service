using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSignatureSheetCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Count_Total",
                table: "CollectionSignatureSheets",
                newName: "Count_Invalid");

            migrationBuilder.RenameColumn(
                name: "PhysicalCount_Total",
                table: "CollectionMunicipalities",
                newName: "PhysicalCount_Invalid");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- citizen service
                                     GRANT SELECT, UPDATE ON "CollectionMunicipalities" TO citizenservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Count_Invalid",
                table: "CollectionSignatureSheets",
                newName: "Count_Total");

            migrationBuilder.RenameColumn(
                name: "PhysicalCount_Invalid",
                table: "CollectionMunicipalities",
                newName: "PhysicalCount_Total");
        }
    }
}

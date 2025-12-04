using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemvoeSheetCountInvalid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count_Invalid",
                table: "CollectionSignatureSheets");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT INSERT,DELETE ON "CollectionCitizens" TO adminservice;
                                     GRANT INSERT,DELETE ON "CollectionCitizenLogs" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count_Invalid",
                table: "CollectionSignatureSheets",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecureIdNumberToCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Collections",
                newName: "SecureIdNumber");

            // generate a new 12 char random string for all rows
            // this overwrites existing "Number" values, which are in an outdated format.
            // The overwriting is acceptable since these are only test values for now.
            migrationBuilder.Sql(
                """
                UPDATE "Collections"
                SET "SecureIdNumber" = (
                    SELECT array_to_string(array_agg(
                        substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', floor(random() * 32)::int + 1, 1)
                    ), '')
                    FROM generate_series(1, 12)
                    WHERE "Collections"."Id" IS NOT NULL
                )
                WHERE NOT "IsElectronicSubmission";

                UPDATE "Collections" SET "SecureIdNumber" = NULL WHERE "IsElectronicSubmission";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_SecureIdNumber",
                table: "Collections",
                column: "SecureIdNumber",
                unique: true,
                filter: "\"SecureIdNumber\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Collections_SecureIdNumber_NotEmpty",
                table: "Collections",
                sql: "\"IsElectronicSubmission\" OR (\n   \"SecureIdNumber\" IS NOT NULL\n   AND \"SecureIdNumber\" <> ''\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_SecureIdNumber",
                table: "Collections");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Collections_SecureIdNumber_NotEmpty",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "SecureIdNumber",
                table: "Collections",
                newName: "Number");
        }
    }
}

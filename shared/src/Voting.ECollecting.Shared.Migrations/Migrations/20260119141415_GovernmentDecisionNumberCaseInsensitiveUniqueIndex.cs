using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class GovernmentDecisionNumberCaseInsensitiveUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Collections_GovernmentDecisionNumber",
                table: "Collections");

            migrationBuilder.AddColumn<string>(
                name: "GovernmentDecisionNumberLower",
                table: "Collections",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(\"GovernmentDecisionNumber\")",
                stored: true);

            // Appends "-copy-{Id}" to duplicates to ensure uniqueness
            migrationBuilder.Sql(
                """
                 UPDATE "Collections"
                 SET "GovernmentDecisionNumber" = "GovernmentDecisionNumber" || '-copy-' || "Id"
                 WHERE "Id" IN (
                     SELECT "Id"
                     FROM (
                         SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "GovernmentDecisionNumberLower" ORDER BY "Id") as rn
                         FROM "Collections"
                         WHERE "GovernmentDecisionNumber" <> '' AND "GovernmentDecisionNumber" IS NOT NULL
                     ) t
                     WHERE t.rn > 1
                 );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_GovernmentDecisionNumberLower",
                table: "Collections",
                column: "GovernmentDecisionNumberLower",
                unique: true,
                filter: "\"GovernmentDecisionNumber\" <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException("Down is not supported by this migration");
        }
    }
}

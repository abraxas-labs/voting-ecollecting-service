using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionUniqueIndexAndLowerColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionLower",
                schema: "ecollecting",
                table: "Decrees",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(\"Description\")",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionLower",
                schema: "ecollecting",
                table: "Collections",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(\"Description\")",
                stored: true);

            // Appends "-copy-{Id}" to duplicates to ensure uniqueness
            migrationBuilder.Sql(
                """
                 UPDATE "Collections"
                 SET "Description" = "Description" || '-copy-' || "Id"
                 WHERE "Id" IN (
                     SELECT "Id"
                     FROM (
                         SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "DescriptionLower" ORDER BY "Id") as rn
                         FROM "Collections"
                         WHERE "Description" <> '' AND "Description" IS NOT NULL
                     ) t
                     WHERE t.rn > 1
                 );
                """);

            // Appends "-copy-{Id}" to duplicates to ensure uniqueness
            migrationBuilder.Sql(
                """
                 UPDATE "Decrees"
                 SET "Description" = "Description" || '-copy-' || "Id"
                 WHERE "Id" IN (
                     SELECT "Id"
                     FROM (
                         SELECT "Id", ROW_NUMBER() OVER (PARTITION BY "DescriptionLower" ORDER BY "Id") as rn
                         FROM "Decrees"
                         WHERE "Description" <> '' AND "Description" IS NOT NULL
                     ) t
                     WHERE t.rn > 1
                 );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Decrees_DescriptionLower_Bfs",
                schema: "ecollecting",
                table: "Decrees",
                columns: new[] { "DescriptionLower", "Bfs" },
                unique: true);

            // use raw sql for creating index since NULLS NOT DISTINCT is not supported in the builder
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_Collections_DescriptionLower_Bfs_DecreeId""
                ON ecollecting.""Collections"" (""DescriptionLower"", ""Bfs"", ""DecreeId"")
                NULLS NOT DISTINCT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Decrees_DescriptionLower_Bfs",
                schema: "ecollecting",
                table: "Decrees");

            migrationBuilder.DropIndex(
                name: "IX_Collections_DescriptionLower_Bfs_DecreeId",
                schema: "ecollecting",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "DescriptionLower",
                schema: "ecollecting",
                table: "Decrees");

            migrationBuilder.DropColumn(
                name: "DescriptionLower",
                schema: "ecollecting",
                table: "Collections");
        }
    }
}

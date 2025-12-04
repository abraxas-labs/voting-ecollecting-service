using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSignatureSheetCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionSignatureSheetNumberCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bfs = table.Column<string>(type: "text", nullable: false),
                    NextNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSignatureSheetNumberCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSignatureSheetNumberCounters_Collections_Collecti~",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheetNumberCounters_Bfs_CollectionId",
                table: "CollectionSignatureSheetNumberCounters",
                columns: new[] { "Bfs", "CollectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheetNumberCounters_CollectionId",
                table: "CollectionSignatureSheetNumberCounters",
                column: "CollectionId");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT SELECT, INSERT, UPDATE ON "CollectionSignatureSheetNumberCounters" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionSignatureSheetNumberCounters");
        }
    }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTrailEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEntityName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    RecordBefore = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RecordAfter = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrailEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCitizenLogAuditTrailEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    RecordBefore = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    RecordAfter = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCitizenLogAuditTrailEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCitizenLogAuditTrailEntries_CollectionCitizenLogs~",
                        column: x => x.SourceEntityId,
                        principalTable: "CollectionCitizenLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogAuditTrailEntries_SourceEntityId",
                table: "CollectionCitizenLogAuditTrailEntries",
                column: "SourceEntityId");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                    -- citizen service
                                    GRANT INSERT ON "AuditTrailEntries" TO citizenservice;
                                    GRANT INSERT ON "CollectionCitizenLogAuditTrailEntries" TO citizenservice;

                                    -- admin service
                                    GRANT INSERT ON "AuditTrailEntries" TO adminservice;
                                    GRANT INSERT ON "CollectionCitizenLogAuditTrailEntries" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrailEntries");

            migrationBuilder.DropTable(
                name: "CollectionCitizenLogAuditTrailEntries");
        }
    }
}

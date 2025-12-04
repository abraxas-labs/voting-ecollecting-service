using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AclImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "text", nullable: false),
                    ImportRecordsCountTotal = table.Column<int>(type: "integer", nullable: false),
                    DatasetsCountCreated = table.Column<int>(type: "integer", nullable: false),
                    DatasetsCountUpdated = table.Column<int>(type: "integer", nullable: false),
                    DatasetsCountDeleted = table.Column<int>(type: "integer", nullable: false),
                    FinishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalElapsedMilliseconds = table.Column<double>(type: "double precision", nullable: true),
                    HasValidationErrors = table.Column<bool>(type: "boolean", nullable: false),
                    EntitiesWithValidationErrors = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    ImportStatus = table.Column<string>(type: "text", nullable: false),
                    ImportType = table.Column<string>(type: "text", nullable: false),
                    WorkerName = table.Column<string>(type: "text", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessControlListDois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Bfs = table.Column<string>(type: "text", nullable: true),
                    TenantName = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Canton = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationErrors = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImportStatisticId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessControlListDois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessControlListDois_AccessControlListDois_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AccessControlListDois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessControlListDois_ImportStatistics_ImportStatisticId",
                        column: x => x.ImportStatisticId,
                        principalTable: "ImportStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessControlListDois_ImportStatisticId",
                table: "AccessControlListDois",
                column: "ImportStatisticId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessControlListDois_ParentId",
                table: "AccessControlListDois",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessControlListDois");

            migrationBuilder.DropTable(
                name: "ImportStatistics");
        }
    }
}

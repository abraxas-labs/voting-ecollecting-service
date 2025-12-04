using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCitizenLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollectionCitizens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    MunicipalityName = table.Column<string>(type: "text", nullable: false),
                    Electronic = table.Column<bool>(type: "boolean", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Sex = table.Column<int>(type: "integer", nullable: false),
                    CollectionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureVersion = table.Column<byte>(type: "smallint", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureKeyId = table.Column<string>(type: "text", nullable: false),
                    IntegritySignatureInfo_IntegritySignature = table.Column<byte[]>(type: "bytea", nullable: false),
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
                    table.PrimaryKey("PK_CollectionCitizens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCitizens_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCitizenLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionCitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingStimmregisterIdEncrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    VotingStimmregisterIdMac = table.Column<byte[]>(type: "bytea", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureVersion = table.Column<byte>(type: "smallint", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureKeyId = table.Column<string>(type: "text", nullable: false),
                    IntegritySignatureInfo_IntegritySignature = table.Column<byte[]>(type: "bytea", nullable: false),
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
                    table.PrimaryKey("PK_CollectionCitizenLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCitizenLogs_CollectionCitizens_CollectionCitizenId",
                        column: x => x.CollectionCitizenId,
                        principalTable: "CollectionCitizens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionCitizenLogs_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogs_CollectionCitizenId",
                table: "CollectionCitizenLogs",
                column: "CollectionCitizenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogs_CollectionId_VotingStimmregisterIdMac",
                table: "CollectionCitizenLogs",
                columns: new[] { "CollectionId", "VotingStimmregisterIdMac" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizens_CollectionId",
                table: "CollectionCitizens",
                column: "CollectionId");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- citizen service
                                     GRANT SELECT, INSERT ON "CollectionCitizens" TO citizenservice;
                                     GRANT SELECT, INSERT ON "CollectionCitizenLogs" TO citizenservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionCitizenLogs");

            migrationBuilder.DropTable(
                name: "CollectionCitizens");
        }
    }
}

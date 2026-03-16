using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeDomainOfInfluencesAndAddSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessControlListDois");

            migrationBuilder.DropTable(
                name: "DomainOfInfluences");

            migrationBuilder.CreateTable(
                name: "DomainOfInfluences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Bfs = table.Column<string>(type: "text", nullable: true),
                    TenantName = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    BasisType = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Canton = table.Column<string>(type: "text", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationErrors = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImportStatisticId = table.Column<Guid>(type: "uuid", nullable: true),
                    ECollectingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortNumber = table.Column<int>(type: "integer", nullable: false),
                    BasisNameForProtocol = table.Column<string>(type: "text", nullable: false),
                    ECollectingNameForProtocol = table.Column<string>(type: "text", nullable: true),
                    LogoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Street = table.Column<string>(type: "text", nullable: false),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    Locality = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NotificationEmails = table.Column<List<string>>(type: "text[]", nullable: false),
                    Webpage = table.Column<string>(type: "text", nullable: true),
                    InitiativeMinSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    InitiativeMaxElectronicSignaturePercent = table.Column<int>(type: "integer", nullable: true),
                    InitiativeNumberOfMembersCommittee = table.Column<int>(type: "integer", nullable: true),
                    ReferendumMinSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    ReferendumMaxElectronicSignaturePercent = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_DomainOfInfluences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainOfInfluences_Files_LogoId",
                        column: x => x.LogoId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DomainOfInfluences_ImportStatistics_ImportStatisticId",
                        column: x => x.ImportStatisticId,
                        principalTable: "ImportStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DomainOfInfluences_DomainOfInfluences_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DomainOfInfluences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_Bfs",
                table: "DomainOfInfluences",
                column: "Bfs",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_ImportStatisticId",
                table: "DomainOfInfluences",
                column: "ImportStatisticId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_LogoId",
                table: "DomainOfInfluences",
                column: "LogoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_ParentId",
                table: "DomainOfInfluences",
                column: "ParentId");

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT SELECT, INSERT, UPDATE, DELETE ON "DomainOfInfluences" TO adminservice;
                                     GRANT SELECT ON "DomainOfInfluences" TO citizenservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainOfInfluences");

            migrationBuilder.CreateTable(
                name: "AccessControlListDois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportStatisticId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Bfs = table.Column<string>(type: "text", nullable: true),
                    Canton = table.Column<string>(type: "text", nullable: false),
                    ECollectingEmail = table.Column<string>(type: "text", nullable: false),
                    ECollectingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ECollectingInitiativeMaxElectronicSignaturePercent = table.Column<int>(type: "integer", nullable: true),
                    ECollectingInitiativeMinSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    ECollectingInitiativeNumberOfMembersCommittee = table.Column<int>(type: "integer", nullable: true),
                    ECollectingReferendumMaxElectronicSignaturePercent = table.Column<int>(type: "integer", nullable: true),
                    ECollectingReferendumMinSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NameForProtocol = table.Column<string>(type: "text", nullable: false),
                    SortNumber = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    TenantName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ValidationErrors = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_ModifiedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "DomainOfInfluences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Bfs = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Locality = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Street = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Webpage = table.Column<string>(type: "text", nullable: true),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_ModifiedByEmail = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainOfInfluences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainOfInfluences_Files_LogoId",
                        column: x => x.LogoId,
                        principalTable: "Files",
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

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_Bfs",
                table: "DomainOfInfluences",
                column: "Bfs",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainOfInfluences_LogoId",
                table: "DomainOfInfluences",
                column: "LogoId",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class CollectionModelChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCounts_Collections_CollectionId",
                table: "CollectionCounts");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.CreateTable(
                name: "CollectionBaseEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false),
                    LogoImageName = table.Column<string>(type: "text", nullable: false),
                    LogoImage = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureSheetGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureSheetFile = table.Column<byte[]>(type: "bytea", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    SensitiveDataExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DomainOfInfluenceType = table.Column<int>(type: "integer", nullable: true),
                    MunicipalityId = table.Column<string>(type: "text", nullable: true),
                    CollectionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollectionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    MaxElectronicSignatureCount = table.Column<int>(type: "integer", nullable: true),
                    IsElectronicSubmission = table.Column<bool>(type: "boolean", nullable: true),
                    Wording = table.Column<string>(type: "text", nullable: true),
                    MembersCommittee = table.Column<string>(type: "text", nullable: true),
                    DecreeId = table.Column<Guid>(type: "uuid", nullable: true),
                    IntegritySignatureInfo_IntegritySignatureVersion = table.Column<byte>(type: "smallint", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureKeyId = table.Column<string>(type: "text", nullable: false),
                    IntegritySignatureInfo_IntegritySignature = table.Column<byte[]>(type: "bytea", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionBaseEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionBaseEntity_Decrees_DecreeId",
                        column: x => x.DecreeId,
                        principalTable: "Decrees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionBaseEntity_DecreeId",
                table: "CollectionBaseEntity",
                column: "DecreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCounts_CollectionBaseEntity_CollectionId",
                table: "CollectionCounts",
                column: "CollectionId",
                principalTable: "CollectionBaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCounts_CollectionBaseEntity_CollectionId",
                table: "CollectionCounts");

            migrationBuilder.DropTable(
                name: "CollectionBaseEntity");

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DecreeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Committee = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DomainOfInfluenceType = table.Column<int>(type: "integer", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    LogoImage = table.Column<byte[]>(type: "bytea", nullable: false),
                    LogoImageName = table.Column<string>(type: "text", nullable: false),
                    MunicipalityId = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    SensitiveDataExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignatureSheetFile = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureSheetGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                    IntegritySignatureInfo_IntegritySignature = table.Column<byte[]>(type: "bytea", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureKeyId = table.Column<string>(type: "text", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureVersion = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Decrees_DecreeId",
                        column: x => x.DecreeId,
                        principalTable: "Decrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_DecreeId",
                table: "Collections",
                column: "DecreeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCounts_Collections_CollectionId",
                table: "CollectionCounts",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

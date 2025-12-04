using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedCollectionForReferendums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DecreeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DomainOfInfluenceType = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Committee = table.Column<string>(type: "text", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false),
                    LogoImageName = table.Column<string>(type: "text", nullable: false),
                    LogoImage = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignatureSheetGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    SignatureSheetFile = table.Column<byte[]>(type: "bytea", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    SensitiveDataExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Decrees_DecreeId",
                        column: x => x.DecreeId,
                        principalTable: "Decrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CollectionCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalCitizenCount = table.Column<int>(type: "integer", nullable: false),
                    ElectronicCitizenCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CollectionCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCounts_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCounts_CollectionId",
                table: "CollectionCounts",
                column: "CollectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Collections_DecreeId",
                table: "Collections",
                column: "DecreeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionCounts");

            migrationBuilder.DropTable(
                name: "Collections");
        }
    }
}

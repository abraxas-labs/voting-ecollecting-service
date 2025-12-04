using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Decrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FederalLevelId = table.Column<int>(type: "integer", nullable: false),
                    CollectionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CollectionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MinSignatureCount = table.Column<int>(type: "integer", nullable: false),
                    MaxElectronicSignatureCount = table.Column<int>(type: "integer", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    DecreeStateId = table.Column<int>(type: "integer", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureVersion = table.Column<byte>(type: "smallint", nullable: false),
                    IntegritySignatureInfo_IntegritySignatureKeyId = table.Column<string>(type: "text", nullable: false),
                    IntegritySignatureInfo_IntegritySignature = table.Column<byte[]>(type: "bytea", nullable: false),
                    AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: false),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decrees", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Decrees");
        }
    }
}

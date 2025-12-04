using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Info_NotBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Info_NotAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Info_Thumbprint = table.Column<string>(type: "text", nullable: true),
                    Info_Subject = table.Column<string>(type: "text", nullable: true),
                    CAInfo_NotBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CAInfo_NotAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CAInfo_Thumbprint = table.Column<string>(type: "text", nullable: true),
                    CAInfo_Subject = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_Files_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ContentId",
                table: "Certificates",
                column: "ContentId",
                unique: true);

            // only one active certificate
            migrationBuilder.Sql("""
                                 create unique index IX_Certificates_Active
                                 on "Certificates" ("Active")
                                 where "Certificates"."Active";
                                 """);

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- citizen service
                                     GRANT SELECT, INSERT, UPDATE ON "Certificates" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}

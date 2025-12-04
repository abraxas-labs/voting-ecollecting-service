using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddCollectionPermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CollectionPermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                Accepted = table.Column<bool>(type: "boolean", nullable: false),
                IamLastName = table.Column<string>(type: "text", nullable: false),
                IamFirstName = table.Column<string>(type: "text", nullable: false),
                IamUserId = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
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
                table.PrimaryKey("PK_CollectionPermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_CollectionPermissions_Collections_CollectionId",
                    column: x => x.CollectionId,
                    principalTable: "Collections",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionPermissions_CollectionId",
            table: "CollectionPermissions",
            column: "CollectionId");

        migrationBuilder.Sql("""
                             DO $$
                             BEGIN
                                 -- admin service
                                 GRANT INSERT, SELECT, DELETE ON "CollectionPermissions" TO adminservice;

                                 -- citizen service
                                 GRANT INSERT, SELECT, DELETE ON "CollectionPermissions" TO citizenservice;
                             END $$;
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CollectionPermissions");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddCollectionMessages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CollectionMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                InitiativeId = table.Column<Guid>(type: "uuid", nullable: true),
                ReferendumId = table.Column<Guid>(type: "uuid", nullable: true),
                Content = table.Column<string>(type: "text", nullable: false),
                AuditInfo_CreatedById = table.Column<string>(type: "text", nullable: false),
                AuditInfo_CreatedByName = table.Column<string>(type: "text", nullable: false),
                AuditInfo_CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AuditInfo_ModifiedById = table.Column<string>(type: "text", nullable: true),
                AuditInfo_ModifiedByName = table.Column<string>(type: "text", nullable: true),
                AuditInfo_ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CollectionMessages", x => x.Id);
                table.ForeignKey(
                    name: "FK_CollectionMessages_CollectionBaseEntity_InitiativeId",
                    column: x => x.InitiativeId,
                    principalTable: "CollectionBaseEntity",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CollectionMessages_CollectionBaseEntity_ReferendumId",
                    column: x => x.ReferendumId,
                    principalTable: "CollectionBaseEntity",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserNotifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RecipientEMail = table.Column<string>(type: "text", nullable: false),
                State = table.Column<int>(type: "integer", nullable: false),
                SentTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                FailureCounter = table.Column<int>(type: "integer", nullable: false),
                LastError = table.Column<string>(type: "text", nullable: true),
                TemplateBag_CollectionName = table.Column<string>(type: "text", nullable: false),
                TemplateBag_RecipientIsCitizen = table.Column<bool>(type: "boolean", nullable: false),
                TemplateBag_CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                TemplateBag_CollectionType = table.Column<int>(type: "integer", nullable: false),
                TemplateBag_NotificationType = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserNotifications", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionMessages_InitiativeId",
            table: "CollectionMessages",
            column: "InitiativeId");

        migrationBuilder.CreateIndex(
            name: "IX_CollectionMessages_ReferendumId",
            table: "CollectionMessages",
            column: "ReferendumId");

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_State",
            table: "UserNotifications",
            column: "State");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionBaseEntity_Decrees_DecreeId",
            table: "CollectionBaseEntity");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionBaseEntity_InitiativeSubTypes_SubTypeId",
            table: "CollectionBaseEntity");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionCounts_CollectionBaseEntity_CollectionId",
            table: "CollectionCounts");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_CollectionBaseEntity_InitiativeId",
            table: "CollectionMessages");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_CollectionBaseEntity_ReferendumId",
            table: "CollectionMessages");

        migrationBuilder.DropPrimaryKey(
            name: "PK_CollectionBaseEntity",
            table: "CollectionBaseEntity");

        migrationBuilder.RenameTable(
            name: "CollectionBaseEntity",
            newName: "Collections");

        migrationBuilder.RenameIndex(
            name: "IX_CollectionBaseEntity_SubTypeId",
            table: "Collections",
            newName: "IX_Collections_SubTypeId");

        migrationBuilder.RenameIndex(
            name: "IX_CollectionBaseEntity_DecreeId",
            table: "Collections",
            newName: "IX_Collections_DecreeId");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Collections",
            table: "Collections",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionCounts_Collections_CollectionId",
            table: "CollectionCounts",
            column: "CollectionId",
            principalTable: "Collections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionMessages_Collections_InitiativeId",
            table: "CollectionMessages",
            column: "InitiativeId",
            principalTable: "Collections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionMessages_Collections_ReferendumId",
            table: "CollectionMessages",
            column: "ReferendumId",
            principalTable: "Collections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Collections_Decrees_DecreeId",
            table: "Collections",
            column: "DecreeId",
            principalTable: "Decrees",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Collections_InitiativeSubTypes_SubTypeId",
            table: "Collections",
            column: "SubTypeId",
            principalTable: "InitiativeSubTypes",
            principalColumn: "Id");

        migrationBuilder.Sql("""
                             DO $$
                             BEGIN
                                 -- admin service
                                 GRANT INSERT, SELECT, UPDATE, DELETE ON "CollectionMessages" TO adminservice;
                                 GRANT INSERT, SELECT, UPDATE ON "UserNotifications" TO adminservice;

                                 -- citizen service
                                 GRANT INSERT, SELECT, UPDATE ON "CollectionMessages" TO citizenservice;
                                 GRANT INSERT ON "UserNotifications" TO citizenservice;
                             END $$;
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CollectionMessages");

        migrationBuilder.DropTable(
            name: "UserNotifications");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionCounts_Collections_CollectionId",
            table: "CollectionCounts");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_Collections_InitiativeId",
            table: "CollectionMessages");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_Collections_ReferendumId",
            table: "CollectionMessages");

        migrationBuilder.DropForeignKey(
            name: "FK_Collections_Decrees_DecreeId",
            table: "Collections");

        migrationBuilder.DropForeignKey(
            name: "FK_Collections_InitiativeSubTypes_SubTypeId",
            table: "Collections");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Collections",
            table: "Collections");

        migrationBuilder.RenameTable(
            name: "Collections",
            newName: "CollectionBaseEntity");

        migrationBuilder.RenameIndex(
            name: "IX_Collections_SubTypeId",
            table: "CollectionBaseEntity",
            newName: "IX_CollectionBaseEntity_SubTypeId");

        migrationBuilder.RenameIndex(
                name: "IX_Collections_DecreeId",
                table: "CollectionBaseEntity",
                newName: "IX_CollectionBaseEntity_DecreeId");

        migrationBuilder.AddPrimaryKey(
                name: "PK_CollectionBaseEntity",
                table: "CollectionBaseEntity",
                column: "Id");

        migrationBuilder.AddForeignKey(
                name: "FK_CollectionBaseEntity_Decrees_DecreeId",
                table: "CollectionBaseEntity",
                column: "DecreeId",
                principalTable: "Decrees",
                principalColumn: "Id");

        migrationBuilder.AddForeignKey(
                name: "FK_CollectionBaseEntity_InitiativeSubTypes_SubTypeId",
                table: "CollectionBaseEntity",
                column: "SubTypeId",
                principalTable: "InitiativeSubTypes",
                principalColumn: "Id");

        migrationBuilder.AddForeignKey(
                name: "FK_CollectionCounts_CollectionBaseEntity_CollectionId",
                table: "CollectionCounts",
                column: "CollectionId",
                principalTable: "CollectionBaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
                name: "FK_CollectionMessages_CollectionBaseEntity_InitiativeId",
                table: "CollectionMessages",
                column: "InitiativeId",
                principalTable: "CollectionBaseEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionMessages_CollectionBaseEntity_ReferendumId",
            table: "CollectionMessages",
            column: "ReferendumId",
            principalTable: "CollectionBaseEntity",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}

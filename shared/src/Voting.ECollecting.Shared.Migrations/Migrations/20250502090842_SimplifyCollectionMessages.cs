using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class SimplifyCollectionMessages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CollectionId",
            table: "CollectionMessages",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.Sql(
            """
            UPDATE "CollectionMessages" SET "CollectionId" = COALESCE("InitiativeId", "ReferendumId");
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_Collections_InitiativeId",
            table: "CollectionMessages");

        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_Collections_ReferendumId",
            table: "CollectionMessages");

        migrationBuilder.DropIndex(
            name: "IX_CollectionMessages_InitiativeId",
            table: "CollectionMessages");

        migrationBuilder.DropIndex(
            name: "IX_CollectionMessages_ReferendumId",
            table: "CollectionMessages");

        migrationBuilder.DropColumn(
            name: "InitiativeId",
            table: "CollectionMessages");

        migrationBuilder.DropColumn(
            name: "ReferendumId",
            table: "CollectionMessages");

        migrationBuilder.CreateIndex(
            name: "IX_CollectionMessages_CollectionId",
            table: "CollectionMessages",
            column: "CollectionId");

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionMessages_Collections_CollectionId",
            table: "CollectionMessages",
            column: "CollectionId",
            principalTable: "Collections",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CollectionMessages_Collections_CollectionId",
            table: "CollectionMessages");

        migrationBuilder.DropIndex(
            name: "IX_CollectionMessages_CollectionId",
            table: "CollectionMessages");

        migrationBuilder.DropColumn(
            name: "CollectionId",
            table: "CollectionMessages");

        migrationBuilder.AddColumn<Guid>(
            name: "InitiativeId",
            table: "CollectionMessages",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ReferendumId",
            table: "CollectionMessages",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_CollectionMessages_InitiativeId",
            table: "CollectionMessages",
            column: "InitiativeId");

        migrationBuilder.CreateIndex(
            name: "IX_CollectionMessages_ReferendumId",
            table: "CollectionMessages",
            column: "ReferendumId");

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
    }
}

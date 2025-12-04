using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddAclDomainOfInfluenceReferendumAndInitiativeProperties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ECollectingMaxElectronicSignaturePercent",
            table: "AccessControlListDois");

        migrationBuilder.DropColumn(
            name: "ECollectingMinSignatureCount",
            table: "AccessControlListDois");

        migrationBuilder.AddColumn<Guid>(
            name: "SubTypeId",
            table: "CollectionBaseEntity",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingInitiativeMaxElectronicSignaturePercent",
            table: "AccessControlListDois",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingInitiativeMinSignatureCount",
            table: "AccessControlListDois",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingInitiativeNumberOfMembersCommittee",
            table: "AccessControlListDois",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingReferendumMaxElectronicSignaturePercent",
            table: "AccessControlListDois",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingReferendumMinSignatureCount",
            table: "AccessControlListDois",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "InitiativeSubTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MunicipalityId = table.Column<string>(type: "text", nullable: false),
                DomainOfInfluenceType = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                MinSignatureCount = table.Column<int>(type: "integer", nullable: false),
                MaxElectronicSignatureCount = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InitiativeSubTypes", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "InitiativeSubTypes",
            columns: new[] { "Id", "Description", "DomainOfInfluenceType", "MaxElectronicSignatureCount", "MinSignatureCount", "MunicipalityId" },
            values: new object[,]
            {
                { new Guid("8926f191-6ba3-475f-9db0-d599b3317358"), "Verfassungsinitiative", 2, 4000, 8000, "3200" },
                { new Guid("9bcaba6c-bc1b-43d6-a59e-620ac2f4872a"), "Gesetzesinitiative", 2, 3000, 6000, "3200" },
                { new Guid("abd22fb4-f5d9-463b-8605-edfc0d93a6a3"), "Einheitsinitiative", 2, 2000, 4000, "3200" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_CollectionBaseEntity_SubTypeId",
            table: "CollectionBaseEntity",
            column: "SubTypeId");

        migrationBuilder.AddForeignKey(
            name: "FK_CollectionBaseEntity_InitiativeSubTypes_SubTypeId",
            table: "CollectionBaseEntity",
            column: "SubTypeId",
            principalTable: "InitiativeSubTypes",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CollectionBaseEntity_InitiativeSubTypes_SubTypeId",
            table: "CollectionBaseEntity");

        migrationBuilder.DropTable(
            name: "InitiativeSubTypes");

        migrationBuilder.DropIndex(
            name: "IX_CollectionBaseEntity_SubTypeId",
            table: "CollectionBaseEntity");

        migrationBuilder.DropColumn(
            name: "SubTypeId",
            table: "CollectionBaseEntity");

        migrationBuilder.DropColumn(
            name: "ECollectingInitiativeMaxElectronicSignaturePercent",
            table: "AccessControlListDois");

        migrationBuilder.DropColumn(
            name: "ECollectingInitiativeMinSignatureCount",
            table: "AccessControlListDois");

        migrationBuilder.DropColumn(
            name: "ECollectingInitiativeNumberOfMembersCommittee",
            table: "AccessControlListDois");

        migrationBuilder.DropColumn(
            name: "ECollectingReferendumMaxElectronicSignaturePercent",
            table: "AccessControlListDois");

        migrationBuilder.DropColumn(
            name: "ECollectingReferendumMinSignatureCount",
            table: "AccessControlListDois");

        migrationBuilder.AddColumn<int>(
            name: "ECollectingMaxElectronicSignaturePercent",
            table: "AccessControlListDois",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "ECollectingMinSignatureCount",
            table: "AccessControlListDois",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}

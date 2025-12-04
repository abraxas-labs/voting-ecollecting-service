using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddInitiativePropertiesAndFederalSubType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GovernmentDecisionNumber",
            table: "CollectionBaseEntity",
            type: "text",
            nullable: true);

        migrationBuilder.InsertData(
            table: "InitiativeSubTypes",
            columns: new[] { "Id", "Description", "DomainOfInfluenceType", "MaxElectronicSignatureCount", "MinSignatureCount", "MunicipalityId" },
            values: new object[] { new Guid("d0c38ef9-0619-4fdc-a859-237bf6f6d1d3"), "Volksinitiative", 1, 50000, 100000, "" });

        migrationBuilder.Sql("""
                             DO $$
                             BEGIN
                                 -- admin service
                                 GRANT SELECT ON "InitiativeSubTypes" TO adminservice;

                                 -- citizen service
                                 GRANT SELECT ON "InitiativeSubTypes" TO citizenservice;
                             END $$;
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "InitiativeSubTypes",
            keyColumn: "Id",
            keyValue: new Guid("d0c38ef9-0619-4fdc-a859-237bf6f6d1d3"));

        migrationBuilder.DropColumn(
            name: "GovernmentDecisionNumber",
            table: "CollectionBaseEntity");
    }
}

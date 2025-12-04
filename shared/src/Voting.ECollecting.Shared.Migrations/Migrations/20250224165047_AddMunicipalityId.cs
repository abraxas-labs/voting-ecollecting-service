// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class AddMunicipalityId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "MunicipalityId",
            table: "Decrees",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "MunicipalityId",
            table: "Collections",
            type: "text",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<bool>(
            name: "ECollectingEnabled",
            table: "AccessControlListDois",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MunicipalityId",
            table: "Decrees");

        migrationBuilder.DropColumn(
            name: "MunicipalityId",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "ECollectingEnabled",
            table: "AccessControlListDois");
    }
}

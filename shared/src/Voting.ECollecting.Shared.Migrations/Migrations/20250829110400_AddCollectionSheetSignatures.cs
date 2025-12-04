using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSheetSignatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeId",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizenLogs_DecreeId_VotingStimmregisterIdMac",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropColumn(
                name: "Electronic",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "MunicipalityId",
                table: "CollectionCitizens");

            migrationBuilder.RenameColumn(
                name: "DecreeId",
                table: "CollectionCitizenLogs",
                newName: "DecreeEntityId");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityBfs",
                table: "CollectionCitizens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SignatureSheetId",
                table: "CollectionCitizens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizens_SignatureSheetId",
                table: "CollectionCitizens",
                column: "SignatureSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogs_DecreeEntityId",
                table: "CollectionCitizenLogs",
                column: "DecreeEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeEntityId",
                table: "CollectionCitizenLogs",
                column: "DecreeEntityId",
                principalTable: "Decrees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizens_CollectionSignatureSheets_SignatureSheet~",
                table: "CollectionCitizens",
                column: "SignatureSheetId",
                principalTable: "CollectionSignatureSheets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT SELECT ON "CollectionCitizens" TO adminservice;
                                     GRANT SELECT ON "CollectionCitizenLogs" TO adminservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeEntityId",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizens_CollectionSignatureSheets_SignatureSheet~",
                table: "CollectionCitizens");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizens_SignatureSheetId",
                table: "CollectionCitizens");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizenLogs_DecreeEntityId",
                table: "CollectionCitizenLogs");

            migrationBuilder.DropColumn(
                name: "MunicipalityBfs",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "SignatureSheetId",
                table: "CollectionCitizens");

            migrationBuilder.RenameColumn(
                name: "DecreeEntityId",
                table: "CollectionCitizenLogs",
                newName: "DecreeId");

            migrationBuilder.AddColumn<bool>(
                name: "Electronic",
                table: "CollectionCitizens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MunicipalityId",
                table: "CollectionCitizens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizenLogs_DecreeId_VotingStimmregisterIdMac",
                table: "CollectionCitizenLogs",
                columns: new[] { "DecreeId", "VotingStimmregisterIdMac" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizenLogs_Decrees_DecreeId",
                table: "CollectionCitizenLogs",
                column: "DecreeId",
                principalTable: "Decrees",
                principalColumn: "Id");
        }
    }
}

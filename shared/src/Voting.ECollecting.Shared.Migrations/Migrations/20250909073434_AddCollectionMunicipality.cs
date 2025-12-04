using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionMunicipality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionSignatureSheets_Collections_CollectionId",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropTable(
                name: "CollectionSignatureSheetNumberCounters");

            migrationBuilder.DropIndex(
                name: "IX_CollectionSignatureSheets_CollectionId_Bfs_Number",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizens_CollectionId",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "Bfs",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropColumn(
                name: "MunicipalityName",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "MunicipalityBfs",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "MunicipalityName",
                table: "CollectionCitizens");

            migrationBuilder.RenameColumn(
                name: "CollectionId",
                table: "CollectionSignatureSheets",
                newName: "CollectionMunicipalityId");

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionMunicipalityId",
                table: "CollectionCitizens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollectionMunicipalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bfs = table.Column<string>(type: "text", nullable: false),
                    MunicipalityName = table.Column<string>(type: "text", nullable: false),
                    PhysicalCount_Total = table.Column<int>(type: "integer", nullable: false),
                    PhysicalCount_Valid = table.Column<int>(type: "integer", nullable: false),
                    ElectronicCitizenCount = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    NextSheetNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CollectionMunicipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionMunicipalities_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheets_CollectionMunicipalityId_Number",
                table: "CollectionSignatureSheets",
                columns: new[] { "CollectionMunicipalityId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizens_CollectionMunicipalityId",
                table: "CollectionCitizens",
                column: "CollectionMunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionMunicipalities_CollectionId_Bfs",
                table: "CollectionMunicipalities",
                columns: new[] { "CollectionId", "Bfs" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizens_CollectionMunicipalities_CollectionMunic~",
                table: "CollectionCitizens",
                column: "CollectionMunicipalityId",
                principalTable: "CollectionMunicipalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionSignatureSheets_CollectionMunicipalities_Collecti~",
                table: "CollectionSignatureSheets",
                column: "CollectionMunicipalityId",
                principalTable: "CollectionMunicipalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                                 DO $$
                                 BEGIN
                                     -- admin service
                                     GRANT SELECT, INSERT, UPDATE ON "CollectionMunicipalities" TO adminservice;

                                     -- citizen service
                                     GRANT SELECT ON "CollectionMunicipalities" TO citizenservice;
                                 END $$;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizens_CollectionMunicipalities_CollectionMunic~",
                table: "CollectionCitizens");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionSignatureSheets_CollectionMunicipalities_Collecti~",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropTable(
                name: "CollectionMunicipalities");

            migrationBuilder.DropIndex(
                name: "IX_CollectionSignatureSheets_CollectionMunicipalityId_Number",
                table: "CollectionSignatureSheets");

            migrationBuilder.DropIndex(
                name: "IX_CollectionCitizens_CollectionMunicipalityId",
                table: "CollectionCitizens");

            migrationBuilder.DropColumn(
                name: "CollectionMunicipalityId",
                table: "CollectionCitizens");

            migrationBuilder.RenameColumn(
                name: "CollectionMunicipalityId",
                table: "CollectionSignatureSheets",
                newName: "CollectionId");

            migrationBuilder.AddColumn<string>(
                name: "Bfs",
                table: "CollectionSignatureSheets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityName",
                table: "CollectionSignatureSheets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionId",
                table: "CollectionCitizens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityBfs",
                table: "CollectionCitizens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityName",
                table: "CollectionCitizens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CollectionSignatureSheetNumberCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Bfs = table.Column<string>(type: "text", nullable: false),
                    NextNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionSignatureSheetNumberCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionSignatureSheetNumberCounters_Collections_Collecti~",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheets_CollectionId_Bfs_Number",
                table: "CollectionSignatureSheets",
                columns: new[] { "CollectionId", "Bfs", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCitizens_CollectionId",
                table: "CollectionCitizens",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheetNumberCounters_Bfs_CollectionId",
                table: "CollectionSignatureSheetNumberCounters",
                columns: new[] { "Bfs", "CollectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionSignatureSheetNumberCounters_CollectionId",
                table: "CollectionSignatureSheetNumberCounters",
                column: "CollectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionSignatureSheets_Collections_CollectionId",
                table: "CollectionSignatureSheets",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

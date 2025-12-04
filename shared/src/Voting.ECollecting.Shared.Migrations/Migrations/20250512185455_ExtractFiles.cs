using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations;

/// <inheritdoc />
public partial class ExtractFiles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Image",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "ImageName",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "LogoImage",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "LogoImageName",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "SignatureSheetFile",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "SignatureSheetName",
            table: "Collections");

        migrationBuilder.AddColumn<bool>(
            name: "HasSignatureSheet",
            table: "Collections",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<Guid>(
            name: "ImageId",
            table: "Collections",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "LogoId",
            table: "Collections",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "SignatureSheetId",
            table: "Collections",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Files",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ContentType = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Files", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "FileContents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Data = table.Column<byte[]>(type: "bytea", nullable: false),
                FileId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FileContents", x => x.Id);
                table.ForeignKey(
                    name: "FK_FileContents_Files_FileId",
                    column: x => x.FileId,
                    principalTable: "Files",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Collections_ImageId",
            table: "Collections",
            column: "ImageId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Collections_LogoId",
            table: "Collections",
            column: "LogoId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Collections_SignatureSheetId",
            table: "Collections",
            column: "SignatureSheetId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FileContents_FileId",
            table: "FileContents",
            column: "FileId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Collections_Files_ImageId",
            table: "Collections",
            column: "ImageId",
            principalTable: "Files",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_Collections_Files_LogoId",
            table: "Collections",
            column: "LogoId",
            principalTable: "Files",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_Collections_Files_SignatureSheetId",
            table: "Collections",
            column: "SignatureSheetId",
            principalTable: "Files",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.DropColumn(
            name: "HasSignatureSheet",
            table: "Collections");

        migrationBuilder.Sql("""
                             DO $$
                             BEGIN
                                 -- admin service
                                 GRANT SELECT ON "Files" TO adminservice;
                                 GRANT SELECT ON "FileContents" TO adminservice;

                                 -- citizen service
                                 GRANT INSERT, SELECT, DELETE ON "Files" TO citizenservice;
                                 GRANT INSERT, SELECT, DELETE ON "FileContents" TO citizenservice;
                             END $$;
                             """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasSignatureSheet",
            table: "Collections",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.DropForeignKey(
            name: "FK_Collections_Files_ImageId",
            table: "Collections");

        migrationBuilder.DropForeignKey(
            name: "FK_Collections_Files_LogoId",
            table: "Collections");

        migrationBuilder.DropForeignKey(
            name: "FK_Collections_Files_SignatureSheetId",
            table: "Collections");

        migrationBuilder.DropTable(
            name: "FileContents");

        migrationBuilder.DropTable(
            name: "Files");

        migrationBuilder.DropIndex(
            name: "IX_Collections_ImageId",
            table: "Collections");

        migrationBuilder.DropIndex(
            name: "IX_Collections_LogoId",
            table: "Collections");

        migrationBuilder.DropIndex(
            name: "IX_Collections_SignatureSheetId",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "HasSignatureSheet",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "ImageId",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "LogoId",
            table: "Collections");

        migrationBuilder.DropColumn(
            name: "SignatureSheetId",
            table: "Collections");

        migrationBuilder.AddColumn<byte[]>(
            name: "Image",
            table: "Collections",
            type: "bytea",
            nullable: false,
            defaultValue: new byte[0]);

        migrationBuilder.AddColumn<string>(
            name: "ImageName",
            table: "Collections",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<byte[]>(
            name: "LogoImage",
            table: "Collections",
            type: "bytea",
            nullable: false,
            defaultValue: new byte[0]);

        migrationBuilder.AddColumn<string>(
            name: "LogoImageName",
            table: "Collections",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<byte[]>(
            name: "SignatureSheetFile",
            table: "Collections",
            type: "bytea",
            nullable: false,
            defaultValue: new byte[0]);

        migrationBuilder.AddColumn<string>(
            name: "SignatureSheetName",
            table: "Collections",
            type: "text",
            nullable: false,
            defaultValue: "");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameSignatureSheetTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Files_SignatureSheetId",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "SignatureSheetId",
                table: "Collections",
                newName: "SignatureSheetTemplateId");

            migrationBuilder.RenameColumn(
                name: "SignatureSheetGenerated",
                table: "Collections",
                newName: "SignatureSheetTemplateGenerated");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_SignatureSheetId",
                table: "Collections",
                newName: "IX_Collections_SignatureSheetTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Files_SignatureSheetTemplateId",
                table: "Collections",
                column: "SignatureSheetTemplateId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Files_SignatureSheetTemplateId",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "SignatureSheetTemplateId",
                table: "Collections",
                newName: "SignatureSheetId");

            migrationBuilder.RenameColumn(
                name: "SignatureSheetTemplateGenerated",
                table: "Collections",
                newName: "SignatureSheetGenerated");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_SignatureSheetTemplateId",
                table: "Collections",
                newName: "IX_Collections_SignatureSheetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Files_SignatureSheetId",
                table: "Collections",
                column: "SignatureSheetId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

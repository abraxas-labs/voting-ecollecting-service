using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class SwitchHmsToKms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pkcs11MacKeyLabel",
                table: "Collections",
                newName: "MacKeyId");

            migrationBuilder.RenameColumn(
                name: "Pkcs11EncryptionKeyLabel",
                table: "Collections",
                newName: "EncryptionKeyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MacKeyId",
                table: "Collections",
                newName: "Pkcs11MacKeyLabel");

            migrationBuilder.RenameColumn(
                name: "EncryptionKeyId",
                table: "Collections",
                newName: "Pkcs11EncryptionKeyLabel");
        }
    }
}

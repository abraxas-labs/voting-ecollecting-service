using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteReferendums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Decrees_DecreeId",
                table: "Collections");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Decrees_DecreeId",
                table: "Collections",
                column: "DecreeId",
                principalTable: "Decrees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Decrees_DecreeId",
                table: "Collections");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Decrees_DecreeId",
                table: "Collections",
                column: "DecreeId",
                principalTable: "Decrees",
                principalColumn: "Id");
        }
    }
}

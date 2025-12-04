using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteCitizenLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCitizens_Collections_CollectionId",
                table: "CollectionCitizens",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

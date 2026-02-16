using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voting.ECollecting.Admin.Adapter.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReceivedAtToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReceivedAt",
                table: "CollectionSignatureSheets",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ReceivedAt",
                table: "CollectionSignatureSheets",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}

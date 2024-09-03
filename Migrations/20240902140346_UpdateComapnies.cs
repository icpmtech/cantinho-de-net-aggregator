using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComapnies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TickerSymbol",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TickerSymbol",
                table: "Companies");
        }
    }
}

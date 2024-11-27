using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class StockExchangesCrudv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StockExchanges",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "StockExchanges");
        }
    }
}

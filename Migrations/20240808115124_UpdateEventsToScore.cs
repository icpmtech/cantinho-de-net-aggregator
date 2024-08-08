using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventsToScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "StockEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryAnalisys",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "SummaryAnalisys",
                table: "StockEvents");
        }
    }
}

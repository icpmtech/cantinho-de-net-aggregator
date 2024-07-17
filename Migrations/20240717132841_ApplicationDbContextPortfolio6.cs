using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationDbContextPortfolio6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends");

            migrationBuilder.DropIndex(
                name: "IX_Dividends_PortfolioId",
                table: "Dividends");

            migrationBuilder.DropColumn(
                name: "PortfolioId",
                table: "Dividends");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortfolioId",
                table: "Dividends",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dividends_PortfolioId",
                table: "Dividends",
                column: "PortfolioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }
    }
}

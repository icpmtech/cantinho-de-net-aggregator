using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationDbContextPortfolio4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Dividends",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PortfolioItemId",
                table: "Dividends",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Dividends_PortfolioItemId",
                table: "Dividends",
                column: "PortfolioItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dividends_PortfolioItems_PortfolioItemId",
                table: "Dividends",
                column: "PortfolioItemId",
                principalTable: "PortfolioItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dividends_PortfolioItems_PortfolioItemId",
                table: "Dividends");

            migrationBuilder.DropForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends");

            migrationBuilder.DropIndex(
                name: "IX_Dividends_PortfolioItemId",
                table: "Dividends");

            migrationBuilder.DropColumn(
                name: "PortfolioItemId",
                table: "Dividends");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Dividends",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dividends_Portfolios_PortfolioId",
                table: "Dividends",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

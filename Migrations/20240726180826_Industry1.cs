using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class Industry1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioItems_Companies_IndustryId",
                table: "PortfolioItems");

            migrationBuilder.RenameColumn(
                name: "IndustryId",
                table: "PortfolioItems",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_PortfolioItems_IndustryId",
                table: "PortfolioItems",
                newName: "IX_PortfolioItems_CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioItems_Companies_CompanyId",
                table: "PortfolioItems",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioItems_Companies_CompanyId",
                table: "PortfolioItems");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "PortfolioItems",
                newName: "IndustryId");

            migrationBuilder.RenameIndex(
                name: "IX_PortfolioItems_CompanyId",
                table: "PortfolioItems",
                newName: "IX_PortfolioItems_IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioItems_Companies_IndustryId",
                table: "PortfolioItems",
                column: "IndustryId",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}

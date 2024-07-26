using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class Industry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndustryId",
                table: "PortfolioItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_IndustryId",
                table: "PortfolioItems",
                column: "IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioItems_Companies_IndustryId",
                table: "PortfolioItems",
                column: "IndustryId",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioItems_Companies_IndustryId",
                table: "PortfolioItems");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioItems_IndustryId",
                table: "PortfolioItems");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "PortfolioItems");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Companies");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class Scraping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorSelector",
                table: "NewsScrapingItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DateSelector",
                table: "NewsScrapingItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionSelector",
                table: "NewsScrapingItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkSelector",
                table: "NewsScrapingItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleSelector",
                table: "NewsScrapingItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorSelector",
                table: "NewsScrapingItem");

            migrationBuilder.DropColumn(
                name: "DateSelector",
                table: "NewsScrapingItem");

            migrationBuilder.DropColumn(
                name: "DescriptionSelector",
                table: "NewsScrapingItem");

            migrationBuilder.DropColumn(
                name: "LinkSelector",
                table: "NewsScrapingItem");

            migrationBuilder.DropColumn(
                name: "TitleSelector",
                table: "NewsScrapingItem");
        }
    }
}

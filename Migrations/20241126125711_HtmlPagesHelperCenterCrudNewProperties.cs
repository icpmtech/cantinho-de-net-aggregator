using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class HtmlPagesHelperCenterCrudNewProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangeHistory",
                table: "HtmlPages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "HtmlPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastEditedBy",
                table: "HtmlPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "HtmlPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "HtmlPages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeHistory",
                table: "HtmlPages");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "HtmlPages");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "HtmlPages");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "HtmlPages");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "HtmlPages");
        }
    }
}

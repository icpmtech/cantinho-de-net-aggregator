using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class Indudtry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                table: "StockEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Calendar",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "StockEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Guests",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "StockEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "StockEvents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllDay",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Calendar",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "End",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Guests",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "StockEvents");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "StockEvents");
        }
    }
}

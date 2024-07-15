using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationDbContextPortfolio2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "News",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualitativeEventId",
                table: "News",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_QualitativeEventId",
                table: "News",
                column: "QualitativeEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_News_QualitativeEvents_QualitativeEventId",
                table: "News",
                column: "QualitativeEventId",
                principalTable: "QualitativeEvents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_QualitativeEvents_QualitativeEventId",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_QualitativeEventId",
                table: "News");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "News");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "News");

            migrationBuilder.DropColumn(
                name: "QualitativeEventId",
                table: "News");
        }
    }
}

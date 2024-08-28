using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeclaredDividendsExdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExDateDividend",
                table: "DividendsTrackers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayDateDividend",
                table: "DividendsTrackers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DividendsTrackers",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ExDateDividend", "PayDateDividend" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExDateDividend",
                table: "DividendsTrackers");

            migrationBuilder.DropColumn(
                name: "PayDateDividend",
                table: "DividendsTrackers");
        }
    }
}

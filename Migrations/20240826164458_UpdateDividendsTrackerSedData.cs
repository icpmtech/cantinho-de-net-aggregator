using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDividendsTrackerSedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DividendsTracker",
                columns: new[] { "Id", "Company", "Country", "Exchange", "PrevDividend", "Region", "SharePrice", "Ticker" },
                values: new object[,]
                {
                    { 3, "Telefonica S.A", "Spain", "XMAD", "15¢", "Europe", "€4.06", "TEF" },
                    { 4, "Apple Inc.", "United States", "NASDAQ", "20¢", "North America", "$150.00", "AAPL" },
                    { 5, "Microsoft Corp", "United States", "NASDAQ", "30¢", "North America", "$280.00", "MSFT" },
                    { 6, "Royal Dutch Shell", "United Kingdom", "LSE", "40p", "United Kingdom", "£20.00", "RDSA" },
                    { 7, "Unilever PLC", "United Kingdom", "LSE", "37p", "United Kingdom", "£43.00", "ULVR" }
                });

            migrationBuilder.UpdateData(
                table: "IndicesDividendsTracker",
                keyColumn: "Id",
                keyValue: 1,
                column: "Indices",
                value: "[\"AEX 25\",\"BBC Global 30\",\"BEL 20\",\"CAC 40\",\"DAX 40\",\"Euronext 100\",\"Euro Stoxx 50\",\"FTSE Eurotop 100\",\"FTSE MIB\",\"IBEX 35\",\"OBX Norway 25\",\"OMX Copenhagen 20\",\"OMX Helsinki 25\",\"OMX Stockholm 30\",\"PSI 20\",\"S\\u0026P Global 100\",\"STOXX600\",\"TSIC Dutch15\",\"TSIC Euro30\"]");

            migrationBuilder.UpdateData(
                table: "IndicesDividendsTracker",
                keyColumn: "Id",
                keyValue: 2,
                column: "Indices",
                value: "[\"S\\u0026P 500\",\"NASDAQ 100\",\"Dow Jones Industrial Average\",\"Russell 2000\"]");

            migrationBuilder.InsertData(
                table: "IndicesDividendsTracker",
                columns: new[] { "Id", "Indices", "Region" },
                values: new object[] { 3, "[\"FTSE 100\",\"FTSE 250\",\"FTSE All-Share\",\"FTSE AIM 100\"]", "United Kingdom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DividendsTracker",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DividendsTracker",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "DividendsTracker",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "DividendsTracker",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DividendsTracker",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "IndicesDividendsTracker",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "IndicesDividendsTracker",
                keyColumn: "Id",
                keyValue: 1,
                column: "Indices",
                value: "[\"IBEX 35\",\"Euro Stoxx 50\"]");

            migrationBuilder.UpdateData(
                table: "IndicesDividendsTracker",
                keyColumn: "Id",
                keyValue: 2,
                column: "Indices",
                value: "[\"S\\u0026P 500\",\"NASDAQ 100\"]");
        }
    }
}

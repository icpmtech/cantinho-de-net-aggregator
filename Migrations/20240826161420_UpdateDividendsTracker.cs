using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDividendsTracker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DividendsTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exchange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SharePrice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrevDividend = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendsTracker", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndicesDividendsTracker",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Indices = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicesDividendsTracker", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DividendIndices",
                columns: table => new
                {
                    DividendsTrackerId = table.Column<int>(type: "int", nullable: false),
                    IndicesDividendsTrackerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendIndices", x => new { x.DividendsTrackerId, x.IndicesDividendsTrackerId });
                    table.ForeignKey(
                        name: "FK_DividendIndices_DividendsTracker_DividendsTrackerId",
                        column: x => x.DividendsTrackerId,
                        principalTable: "DividendsTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DividendIndices_IndicesDividendsTracker_IndicesDividendsTrackerId",
                        column: x => x.IndicesDividendsTrackerId,
                        principalTable: "IndicesDividendsTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DividendsTracker",
                columns: new[] { "Id", "Company", "Country", "Exchange", "PrevDividend", "Region", "SharePrice", "Ticker" },
                values: new object[,]
                {
                    { 1, "Banco de Sabadell, S.A.", "Spain", "XVAL", "3¢", "Europe", "€1.80", "SAB" },
                    { 2, "Industria De Diseno Textil SA", "Spain", "XMAD", "19.6¢", "Europe", "€49.40", "ITX" }
                });

            migrationBuilder.InsertData(
                table: "IndicesDividendsTracker",
                columns: new[] { "Id", "Indices", "Region" },
                values: new object[,]
                {
                    { 1, "[\"IBEX 35\",\"Euro Stoxx 50\"]", "Europe" },
                    { 2, "[\"S\\u0026P 500\",\"NASDAQ 100\"]", "North America" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DividendIndices_IndicesDividendsTrackerId",
                table: "DividendIndices",
                column: "IndicesDividendsTrackerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DividendIndices");

            migrationBuilder.DropTable(
                name: "DividendsTracker");

            migrationBuilder.DropTable(
                name: "IndicesDividendsTracker");
        }
    }
}

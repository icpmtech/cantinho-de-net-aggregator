using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class StockExchangesCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockExchanges",
                columns: table => new
                {
                    StockExchangeName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    MIC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketCapUsdTrillion = table.Column<double>(type: "float", nullable: true),
                    MonthlyTradeVolumeUsdBillion = table.Column<double>(type: "float", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UtcOffset = table.Column<double>(type: "float", nullable: false),
                    DST = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursLocalId = table.Column<int>(type: "int", nullable: false),
                    OpenHoursLocal_Id = table.Column<int>(type: "int", nullable: false),
                    OpenHoursLocal_Open = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursLocal_Close = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursLocal_Lunch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursUTCId = table.Column<int>(type: "int", nullable: false),
                    OpenHoursUTC_Id = table.Column<int>(type: "int", nullable: false),
                    OpenHoursUTC_Open = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursUTC_Close = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenHoursUTC_Lunch = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockExchanges", x => x.StockExchangeName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockExchanges");
        }
    }
}

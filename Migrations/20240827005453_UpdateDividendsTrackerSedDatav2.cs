using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDividendsTrackerSedDatav2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_DividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_IndexDividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropTable(
                name: "DividendIndexs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsScrapingItem",
                table: "NewsScrapingItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IndicesDividendsTracker",
                table: "IndicesDividendsTracker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DividendsTracker",
                table: "DividendsTracker");

            migrationBuilder.RenameTable(
                name: "NewsScrapingItem",
                newName: "NewsScrapingItems");

            migrationBuilder.RenameTable(
                name: "IndicesDividendsTracker",
                newName: "IndexDividendsTrackers");

            migrationBuilder.RenameTable(
                name: "DividendsTracker",
                newName: "DividendsTrackers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsScrapingItems",
                table: "NewsScrapingItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IndexDividendsTrackers",
                table: "IndexDividendsTrackers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DividendsTrackers",
                table: "DividendsTrackers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_DividendsTrackers_DividendsTrackerId",
                table: "DividendIndices",
                column: "DividendsTrackerId",
                principalTable: "DividendsTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_IndexDividendsTrackers_IndexDividendsTrackerId",
                table: "DividendIndices",
                column: "IndexDividendsTrackerId",
                principalTable: "IndexDividendsTrackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_DividendsTrackers_DividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_IndexDividendsTrackers_IndexDividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsScrapingItems",
                table: "NewsScrapingItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IndexDividendsTrackers",
                table: "IndexDividendsTrackers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DividendsTrackers",
                table: "DividendsTrackers");

            migrationBuilder.RenameTable(
                name: "NewsScrapingItems",
                newName: "NewsScrapingItem");

            migrationBuilder.RenameTable(
                name: "IndexDividendsTrackers",
                newName: "IndicesDividendsTracker");

            migrationBuilder.RenameTable(
                name: "DividendsTrackers",
                newName: "DividendsTracker");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsScrapingItem",
                table: "NewsScrapingItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IndicesDividendsTracker",
                table: "IndicesDividendsTracker",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DividendsTracker",
                table: "DividendsTracker",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DividendIndexs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DividendsTrackerId = table.Column<int>(type: "int", nullable: false),
                    IndexDividendsTrackerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DividendIndexs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DividendIndexs_DividendsTracker_DividendsTrackerId",
                        column: x => x.DividendsTrackerId,
                        principalTable: "DividendsTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DividendIndexs_IndicesDividendsTracker_IndexDividendsTrackerId",
                        column: x => x.IndexDividendsTrackerId,
                        principalTable: "IndicesDividendsTracker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DividendIndexs_DividendsTrackerId",
                table: "DividendIndexs",
                column: "DividendsTrackerId");

            migrationBuilder.CreateIndex(
                name: "IX_DividendIndexs_IndexDividendsTrackerId",
                table: "DividendIndexs",
                column: "IndexDividendsTrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_DividendsTrackerId",
                table: "DividendIndices",
                column: "DividendsTrackerId",
                principalTable: "DividendsTracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_IndexDividendsTrackerId",
                table: "DividendIndices",
                column: "IndexDividendsTrackerId",
                principalTable: "IndicesDividendsTracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

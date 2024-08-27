using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_DividendsTracker_DividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_IndicesDividendsTracker_IndicesDividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.RenameColumn(
                name: "IndicesDividendsTrackerId",
                table: "DividendIndices",
                newName: "IndexDividendsTrackerId");

            migrationBuilder.RenameIndex(
                name: "IX_DividendIndices_IndicesDividendsTrackerId",
                table: "DividendIndices",
                newName: "IX_DividendIndices_IndexDividendsTrackerId");

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

            migrationBuilder.InsertData(
                table: "DividendIndices",
                columns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                values: new object[,]
                {
                    { 3, 1 },
                    { 4, 2 },
                    { 5, 2 },
                    { 6, 3 },
                    { 7, 3 }
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_DividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropForeignKey(
                name: "FK_DividendIndices_IndexDividendsTrackerId",
                table: "DividendIndices");

            migrationBuilder.DropTable(
                name: "DividendIndexs");

            migrationBuilder.DeleteData(
                table: "DividendIndices",
                keyColumns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "DividendIndices",
                keyColumns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                table: "DividendIndices",
                keyColumns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "DividendIndices",
                keyColumns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "DividendIndices",
                keyColumns: new[] { "DividendsTrackerId", "IndexDividendsTrackerId" },
                keyValues: new object[] { 7, 3 });

            migrationBuilder.RenameColumn(
                name: "IndexDividendsTrackerId",
                table: "DividendIndices",
                newName: "IndicesDividendsTrackerId");

            migrationBuilder.RenameIndex(
                name: "IX_DividendIndices_IndexDividendsTrackerId",
                table: "DividendIndices",
                newName: "IX_DividendIndices_IndicesDividendsTrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_DividendsTracker_DividendsTrackerId",
                table: "DividendIndices",
                column: "DividendsTrackerId",
                principalTable: "DividendsTracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DividendIndices_IndicesDividendsTracker_IndicesDividendsTrackerId",
                table: "DividendIndices",
                column: "IndicesDividendsTrackerId",
                principalTable: "IndicesDividendsTracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

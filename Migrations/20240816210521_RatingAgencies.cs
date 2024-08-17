using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class RatingAgencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditRatingAgencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditRatingAgencies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CreditRatingAgencies",
                columns: new[] { "Id", "Country", "Description", "Name", "Website" },
                values: new object[,]
                {
                    { 1, "United States", "Moody's is a leading global provider of credit ratings, research, and risk analysis.", "Moody's", "https://www.moodys.com/" },
                    { 2, "United States", "S&P Global Ratings is known for providing credit ratings, research, and insights essential to global markets.", "Standard & Poor's (S&P)", "https://www.standardandpoors.com/" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditRatingAgencies");
        }
    }
}

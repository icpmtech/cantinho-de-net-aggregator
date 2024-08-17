using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalyticHub.Migrations
{
    /// <inheritdoc />
    public partial class RuleUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "PortfolioLossRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioLossRules_UserProfileId",
                table: "PortfolioLossRules",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioLossRules_UserProfiles_UserProfileId",
                table: "PortfolioLossRules",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioLossRules_UserProfiles_UserProfileId",
                table: "PortfolioLossRules");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioLossRules_UserProfileId",
                table: "PortfolioLossRules");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "PortfolioLossRules");
        }
    }
}

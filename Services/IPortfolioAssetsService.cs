using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Services
{
  public interface IPortfolioAssetsService
  {
    Task<List<AssetDto>> GetAssetsAsync(string userId, string sortBy = "value");
    Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId, string period);
   
  }
}

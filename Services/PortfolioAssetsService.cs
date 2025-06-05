using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketAnalyticHub.Services
{
  public class PortfolioAssetsService : IPortfolioAssetsService
  {
    private readonly ApplicationDbContext _context;

    public PortfolioAssetsService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<List<AssetDto>> GetAssetsAsync(string userId, string sortBy = "value")
    {

          var assetDtos = await _context.PortfolioItems
       .Where(p => p.UserId == userId)
       .GroupBy(a => a.Symbol)
       .Select(g => new AssetDto
       {
         // pick the first (or Min) Id in the group:
         Id = g.OrderBy(a => a.Id).First().Id,

         Ticker = g.Key,
         Name = g.Key,

         // pick the first non-null sector, or default:
         Sector =  "Empty",

         // average price across lots (or g.Max / g.Min if you prefer)
         Price = g.Average(a => a.CurrentPrice),

         // total quantity
         Quantity = g.Sum(a => a.Quantity)
       })
       .ToListAsync();

      // Sort based on the parameter
      switch (sortBy.ToLower())
      {
        case "value":
          assetDtos = assetDtos.OrderByDescending(a => a.TotalValue).ToList();
          break;
        case "performance":
          assetDtos = assetDtos.OrderByDescending(a => a.Variations.GetValueOrDefault("today", 0)).ToList();
          break;
        case "alphabetical":
          assetDtos = assetDtos.OrderBy(a => a.Name).ToList();
          break;
        default:
          assetDtos = assetDtos.OrderByDescending(a => a.TotalValue).ToList();
          break;
      }

      return assetDtos;
    }

    public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId, string period)
    {
      var assets = await GetAssetsAsync(userId);

      if (assets.Count == 0)
      {
        return new PortfolioSummaryDto
        {
          TotalValue = 0,
          Performance = 0,
          Period = period,
          TopPerformerTicker = "-",
          TopPerformerValue = 0,
          WorstPerformerTicker = "-",
          WorstPerformerValue = 0
        };
      }

      decimal totalValue = assets.Sum(a => a.TotalValue);

      // Calculate weighted average performance
      decimal weightedPerformance = 0;
      foreach (var asset in assets)
      {
        if (asset.Variations.TryGetValue(period, out decimal performanceValue))
        {
          decimal weight = asset.TotalValue / totalValue;
          weightedPerformance += performanceValue * weight;
        }
      }

      // Find top and worst performers
      var assetsWithPerformance = assets
          .Where(a => a.Variations.ContainsKey(period))
          .ToList();

      if (assetsWithPerformance.Count == 0)
      {
        return new PortfolioSummaryDto
        {
          TotalValue = totalValue,
          Performance = 0,
          Period = period,
          TopPerformerTicker = "-",
          TopPerformerValue = 0,
          WorstPerformerTicker = "-",
          WorstPerformerValue = 0
        };
      }

      var topPerformer = assetsWithPerformance
          .OrderByDescending(a => a.Variations[period])
          .First();

      var worstPerformer = assetsWithPerformance
          .OrderBy(a => a.Variations[period])
          .First();

      return new PortfolioSummaryDto
      {
        TotalValue = totalValue,
        Performance = weightedPerformance,
        Period = period,
        TopPerformerTicker = topPerformer.Ticker,
        TopPerformerValue = topPerformer.Variations[period],
        WorstPerformerTicker = worstPerformer.Ticker,
        WorstPerformerValue = worstPerformer.Variations[period]
      };
    }
  }
}

using DocumentFormat.OpenXml.Spreadsheet;
using MarketAnalyticHub.Models.Portfolio.Entities;
using System.Collections.Generic;
using static MarketAnalyticHub.Models.Portfolio.Portfolio;

namespace MarketAnalyticHub.Models.Portfolio
{
  public static class  PortfolioBaseService
  {
    public static IDictionary<DateTime, MonthlyPortfolioSummary> CalculateMonthlySummaries(IEnumerable<PortfolioItem> Items)
    {
      var monthlySummaries = Items
          .GroupBy(item => new DateTime(item.PurchaseDate.Year, item.PurchaseDate.Month, 1))
          .ToDictionary(
              g => g.Key,
              g => new MonthlyPortfolioSummary
              {
                TotalInvestment = g.Sum(item => item.TotalInvestment),
                CurrentMarketValue = g.Sum(item => item.CurrentMarketValue),
                TotalDividendIncome = g.Sum(item => item.TotalDividendIncome)
              });

      return monthlySummaries;
    }

    public static IDictionary<DateTime, WeeklyPortfolioSummary> CalculateWeeklySummaries(IEnumerable<PortfolioItem> Items)
    {
      var weeklySummaries = Items
          .GroupBy(item => PortfolioHelpers.GetStartOfWeek(item.PurchaseDate))
          .ToDictionary(
              g => g.Key,
              g => new WeeklyPortfolioSummary
              {
                TotalInvestment = g.Sum(item => item.TotalInvestment),
                CurrentMarketValue = g.Sum(item => item.CurrentMarketValue),
                TotalDividendIncome = g.Sum(item => item.TotalDividendIncome)
              });

      return weeklySummaries;
    }

    public static IDictionary<DateTime, (WeeklyPortfolioSummary Summary, decimal PercentageChange)> CalculateWeeklySummariesWithChange(IEnumerable<PortfolioItem> Items)
    {
      var weeklySummaries = CalculateWeeklySummaries(Items);

      var result = new Dictionary<DateTime, (WeeklyPortfolioSummary, decimal)>();

      DateTime? previousWeek = null;
      foreach (var week in weeklySummaries.OrderBy(ws => ws.Key))
      {
        decimal percentageChange = 0;
        if (previousWeek.HasValue)
        {
          var previousSummary = weeklySummaries[previousWeek.Value];
          var currentSummary = week.Value;

          if (previousSummary.CurrentMarketValue != 0)
          {
            percentageChange = ((currentSummary.CurrentMarketValue - previousSummary.CurrentMarketValue) / previousSummary.CurrentMarketValue) * 100;
          }
        }

        result.Add(week.Key, (week.Value, percentageChange));
        previousWeek = week.Key;
      }

      return result;
    }
  }
}

using MarketAnalyticHub.Models.Portfolio;
using static MarketAnalyticHub.Models.Portfolio.Portfolio;

public static class PortfolioHelpers
{

  public static DateTime GetStartOfWeek(DateTime date)
  {
    // Assuming week starts on Monday
    int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
    return date.AddDays(-1 * diff).Date;
  }
  public static string GetIconForTransaction(string symbol)
  {
    // You can map symbols to specific icons if needed
    return symbol switch
    {
      "AAPL" => "/img/icons/unicons/apple.png",
      "GOOGL" => "/img/icons/unicons/google.png",
      _ => "/img/icons/unicons/wallet.png",
    };
  }
  public static IDictionary<DateTime, (WeeklyPortfolioSummary Summary, decimal PercentageChange)> AggregateWeeklySummaries(IEnumerable<Portfolio> portfolios)
  {
    var allWeeklySummaries = new Dictionary<DateTime, (WeeklyPortfolioSummary Summary, decimal PercentageChange)>();

    foreach (var portfolio in portfolios)
    {
      var weeklySummaries = PortfolioBaseService.CalculateWeeklySummariesWithChange(portfolio.Items);
      foreach (var summary in weeklySummaries)
      {
        if (allWeeklySummaries.ContainsKey(summary.Key))
        {
          var existingSummary = allWeeklySummaries[summary.Key].Summary;
          existingSummary.TotalInvestment += summary.Value.Summary.TotalInvestment;
          existingSummary.CurrentMarketValue += summary.Value.Summary.CurrentMarketValue;
          existingSummary.TotalDividendIncome += summary.Value.Summary.TotalDividendIncome;

          // Update the percentage change to the latest one since they should be the same
          allWeeklySummaries[summary.Key] = (existingSummary, summary.Value.PercentageChange);
        }
        else
        {
          allWeeklySummaries[summary.Key] = summary.Value;
        }
      }
    }

    return allWeeklySummaries;
  }
  

}

// File: Models/StockViewModel.cs
using System;
using System.Collections.Generic;

namespace MarketAnalyticHub.Models
{
  public class StockViewModel
  {
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public double? Price { get; set; }
    public double? Change { get; set; }
    public long? MarketCap { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public string Description { get; set; }
    public string CEO { get; set; }
    public List<ChartDataPoint> ChartData { get; set; }
    public List<NewsItemScreener> News { get; set; }
    public double SentimentScore { get; set; }

    // New Financial Details
    public double? PERatio { get; set; }          // Price-Earnings Ratio
    public double? EPS { get; set; }             // Earnings Per Share
    public double? FiftyTwoWeekHigh { get; set; } // 52-Week High
    public double? FiftyTwoWeekLow { get; set; }  // 52-Week Low
    public long?   Volume { get; set; }              // Trading Volume
    public double? DividendYield { get; set; }     // Dividend Yield (%)
  }


  public class ChartDataPoint
  {
    public DateTime Date { get; set; }
    public decimal Close { get; set; }
  }

  public class NewsItemScreener
  {
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Source { get; set; }
  }
}

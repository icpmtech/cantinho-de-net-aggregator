// File: Models/StockViewModel.cs
using System;
using System.Collections.Generic;

namespace MarketAnalyticHub.Models
{
  public class StockViewModel
  {
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public decimal Price { get; set; }
    public decimal Change { get; set; } // Percentage change
    public long MarketCap { get; set; } // Market capitalization in dollars

    // New Properties
    public string Sector { get; set; }
    public string Industry { get; set; }
    public string Description { get; set; }
    public string CEO { get; set; }

    // Chart Data (Mock data points)
    public List<ChartDataPoint> ChartData { get; set; }

    // News Articles
    public List<NewsItemScreener> News { get; set; }

    // Sentiment Scores
    public double SentimentScore { get; set; } // Example: -1 to 1
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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarketAnalyticHub.Models.News
{
  public class NewsItem
  {
    public int? Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Date { get; set; }
    public string Link { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public double? Sentiment { get; set; }
    public List<string>? Keywords { get; set; }
    public string? SentimentImpact { get; set; }
    public string? IndustriesImpact { get; internal set; }
    public string? Summary { get; internal set; }

    [NotMapped]
    public string Analysis
    {
      get
      {
        if (string.IsNullOrEmpty(Summary))
          return string.Empty;

        var analyses = ParseStockAnalyses(Summary);
        return JsonConvert.SerializeObject(analyses, Formatting.Indented);
      }
    }

    [NotMapped]
    public List<string> Tickers
    {
      get
      {
        if (string.IsNullOrEmpty(Summary))
          return new List<string>();

        var analyses = ParseStockAnalyses(Summary);
        return analyses.Select(t => t.Ticker).ToList();
      }
    }

    [NotMapped]
    public List<int> Scores
    {
      get
      {
        if (string.IsNullOrEmpty(Summary))
          return new List<int>();

        var analyses = ParseStockAnalyses(Summary);
        return analyses.Select(t => t.Score).ToList();
      }
    }
    [NotMapped]
    public string SentimentAnalisys { get; internal set; }

    private List<StockAnalysis> ParseStockAnalyses(string input)
    {
      var analyses = new List<StockAnalysis>();
      var pattern = new Regex(@"(?<ticker>[\w.]+)-score: (?<score>\d), analysisSummary: (?:""(?<summary>[^""]+)""|(?<summary>[^,]+))(?:,|$)");

      var matches = pattern.Matches(input);
      foreach (Match match in matches)
      {
        var analysis = new StockAnalysis
        {
          Ticker = match.Groups["ticker"].Value,
          Score = int.Parse(match.Groups["score"].Value),
          AnalysisSummary = match.Groups["summary"].Value.Trim()
        };
        analyses.Add(analysis);
      }

      return analyses;
    }
  }
  

  public class StockAnalysis
  {
    public string Ticker { get; set; }
    public int Score { get; set; }
    public string AnalysisSummary { get; set; }
  }
}

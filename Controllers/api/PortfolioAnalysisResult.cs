using MarketAnalyticHub.Models.News;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.api
{
  internal class PortfolioAnalysisResult
  {
    public IEnumerable<NewsItem> Articles { get; set; }
    public List<NewsItem> SimilarEvents { get; set; }
    public Dictionary<string, string> SentimentAnalysis { get; set; }
  }
}

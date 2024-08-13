
using NewsAPI.Models;

namespace MarketAnalyticHub.Services
{
  internal class NewsApiResponse
  {
    public IEnumerable<Article> Articles { get; internal set; }
  }
}

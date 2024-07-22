
using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Controllers
{
  public class SocialSentimentService
  {
   
    public async Task<SocialSentiment> GetSocialSentimentAsync(string ticker)
    {
      return new SocialSentiment();
    }

    public class SocialSentiment
    {
      public double PositiveScore { get; set; }
      public double NegativeScore { get; set; }
    }

  }
}

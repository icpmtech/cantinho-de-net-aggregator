using VaderSharp2;

namespace MarketAnalyticHub.Services
{

  public class SentimentAnalysisService
  {
    private readonly SentimentIntensityAnalyzer _analyzer;

    public SentimentAnalysisService()
    {
      _analyzer = new SentimentIntensityAnalyzer();
    }

    public Task<SentimentResult> AnalyzeSentimentAsync(string text)
    {
      var results = _analyzer.PolarityScores(text);
      
      var sentimentResult = new SentimentResult
      {
        Compound = results.Compound,
        Positive = results.Positive,
        Neutral = results.Neutral,
        Negative = results.Negative
      };

      return Task.FromResult(sentimentResult);
    }
  }
}

namespace MarketAnalyticHub.Controllers.api
{
  using System;

  namespace InvestmentPredictionAPI.Services
  {
    public class InvestmentService
    {
      public InvestmentResult PredictInvestment(InvestmentInput input)
      {
        double growthRate = GetGrowthRate(input.InvestmentType);
        double predictedValue = input.Amount * Math.Pow(1 + growthRate, input.Duration);
        string[] suggestedTickers = GetSuggestedTickers(input.InvestmentType, input.Market);

        return new InvestmentResult
        {
          PredictedValue = predictedValue,
          SuggestedTickers = suggestedTickers
        };
      }

      private double GetGrowthRate(string investmentType)
      {
        return investmentType switch
        {
          "stocks" => 0.07,
          "bonds" => 0.03,
          "real-estate" => 0.05,
          "crypto" => 0.12,
          _ => 0.05
        };
      }

      private string[] GetSuggestedTickers(string investmentType, string market)
      {
        if (market == "us")
        {
          return investmentType switch
          {
            "stocks" => new[] { "AAPL", "GOOGL", "AMZN" },
            "bonds" => new[] { "US10Y", "US30Y", "AGG" },
            "real-estate" => new[] { "VNQ", "SPG", "AMT" },
            "crypto" => new[] { "BTC", "ETH", "BNB" },
            _ => Array.Empty<string>()
          };
        }
        else if (market == "europe")
        {
          return investmentType switch
          {
            "stocks" => new[] { "VOW3.DE", "SIE.DE", "NOVN.SW" },
            "bonds" => new[] { "BUND", "GGB", "FR10Y" },
            "real-estate" => new[] { "WDP.BR", "URW.AS", "SEG.PA" },
            "crypto" => new[] { "BTC", "ETH", "BNB" },
            _ => Array.Empty<string>()
          };
        }
        else if (market == "asia")
        {
          return investmentType switch
          {
            "stocks" => new[] { "9988.HK", "6758.T", "TSMC" },
            "bonds" => new[] { "JP10Y", "CGB", "INB" },
            "real-estate" => new[] { "AREIT", "Link REIT", "Suntec" },
            "crypto" => new[] { "BTC", "ETH", "BNB" },
            _ => Array.Empty<string>()
          };
        }

        return Array.Empty<string>();
      }
    }
  }

}

namespace MarketAnalyticHub.Services
{
  public class FinnhubCandleResponse
  {
    public List<long> T { get; set; } // Timestamps
    public List<decimal> O { get; set; } // Open prices
    public List<decimal> H { get; set; } // High prices
    public List<decimal> L { get; set; } // Low prices
    public List<decimal> C { get; set; } // Close prices

    public List<CandlestickData> ToCandlestickData()
    {
      var result = new List<CandlestickData>();

      for (int i = 0; i < T.Count; i++)
      {
        result.Add(new CandlestickData
        {
          Date = DateTimeOffset.FromUnixTimeSeconds(T[i]).UtcDateTime,
          Open = O[i],
          High = H[i],
          Low = L[i],
          Close = C[i]
        });
      }

      return result;
    }
  }
}

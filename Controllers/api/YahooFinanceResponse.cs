namespace MarketAnalyticHub.Controllers.api
{
  public class YahooFinanceResponse
  {
    public Chart Chart { get; set; }
  }

  public class Chart
  {
    public Result[] Result { get; set; }
    public object Error { get; set; }
  }

  public class Result
  {
    public Meta Meta { get; set; }
    public int[] Timestamp { get; set; }
    public Indicators Indicators { get; set; }
  }

  public class Meta
  {
    public string Currency { get; set; }
    public string Symbol { get; set; }
    public string ExchangeName { get; set; }
    public string InstrumentType { get; set; }
    public int FirstTradeDate { get; set; }
    public int RegularMarketTime { get; set; }
    public int Gmtoffset { get; set; }
    public string Timezone { get; set; }
    public string ExchangeTimezoneName { get; set; }
    public double RegularMarketPrice { get; set; }
    public double ChartPreviousClose { get; set; }
    public int[] PreviousClose { get; set; }
    public double Scale { get; set; }
    public int PriceHint { get; set; }
  }

  public class Indicators
  {
    public Quote[] Quote { get; set; }
  }

  public class Quote
  {
    public double[] Open { get; set; }
    public double[] High { get; set; }
    public double[] Low { get; set; }
    public double[] Close { get; set; }
    public int[] Volume { get; set; }
  }

}

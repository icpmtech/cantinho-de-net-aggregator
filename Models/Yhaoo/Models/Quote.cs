namespace MarketAnalyticHub.Models.Yhaoo.Models
{

  public class Quote
  {
    public string language { get; set; }
    public string region { get; set; }
    public string quoteType { get; set; }
    public string typeDisp { get; set; }
    public string quoteSourceName { get; set; }
    public bool triggerable { get; set; }
    public string customPriceAlertConfidence { get; set; }
    public decimal regularMarketChangePercent { get; set; }
    public string marketState { get; set; }
    public string exchange { get; set; }
    public string exchangeTimezoneName { get; set; }
    public string exchangeTimezoneShortName { get; set; }
    public int gmtOffSetMilliseconds { get; set; }
    public string market { get; set; }
    public bool esgPopulated { get; set; }
    public decimal regularMarketPrice { get; set; }
    public string currency { get; set; }
    public long preMarketTime { get; set; }
    public long postMarketTime { get; set; }
    public long regularMarketTime { get; set; }
    public bool hasPrePostMarketData { get; set; }
    public long firstTradeDateMilliseconds { get; set; }
    public int priceHint { get; set; }
    public decimal regularMarketChange { get; set; }
    public decimal regularMarketPreviousClose { get; set; }
    public string fullExchangeName { get; set; }
    public int sourceInterval { get; set; }
    public int exchangeDataDelayedBy { get; set; }
    public bool tradeable { get; set; }
    public bool cryptoTradeable { get; set; }
    public string symbol { get; set; }
  }
}

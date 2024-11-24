using System.Text.Json.Serialization;

namespace MarketAnalyticHub.Services.ApiDataApp.Services
{
  // Models/PriceDetails.cs
  using System.Text.Json.Serialization;

    public class PostMarketChangePercent
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class PostMarketChange
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class PostMarketPrice
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketChangePercent
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketChange
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class PriceHint
    {
      [JsonPropertyName("raw")]
      public int Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }

      [JsonPropertyName("longFmt")]
      public string LongFmt { get; set; }
    }

    public class RegularMarketPrice
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketDayHigh
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketDayLow
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketVolume
    {
      [JsonPropertyName("raw")]
      public long Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }

      [JsonPropertyName("longFmt")]
      public string LongFmt { get; set; }
    }

    public class RegularMarketPreviousClose
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class RegularMarketOpen
    {
      [JsonPropertyName("raw")]
      public double Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }
    }

    public class MarketCap
    {
      [JsonPropertyName("raw")]
      public long Raw { get; set; }

      [JsonPropertyName("fmt")]
      public string Fmt { get; set; }

      [JsonPropertyName("longFmt")]
      public string LongFmt { get; set; }
    }

    public class Price
    {
      [JsonPropertyName("maxAge")]
      public int MaxAge { get; set; }

      [JsonPropertyName("preMarketChange")]
      public object PreMarketChange { get; set; }

      [JsonPropertyName("preMarketPrice")]
      public object PreMarketPrice { get; set; }

      [JsonPropertyName("preMarketSource")]
      public string PreMarketSource { get; set; }

      [JsonPropertyName("postMarketChangePercent")]
      public PostMarketChangePercent PostMarketChangePercent { get; set; }

      [JsonPropertyName("postMarketChange")]
      public PostMarketChange PostMarketChange { get; set; }

      [JsonPropertyName("postMarketTime")]
      public long PostMarketTime { get; set; }

      [JsonPropertyName("postMarketPrice")]
      public PostMarketPrice PostMarketPrice { get; set; }

      [JsonPropertyName("postMarketSource")]
      public string PostMarketSource { get; set; }

      [JsonPropertyName("regularMarketChangePercent")]
      public RegularMarketChangePercent RegularMarketChangePercent { get; set; }

      [JsonPropertyName("regularMarketChange")]
      public RegularMarketChange RegularMarketChange { get; set; }

      [JsonPropertyName("regularMarketTime")]
      public long RegularMarketTime { get; set; }

      [JsonPropertyName("priceHint")]
      public PriceHint PriceHint { get; set; }

      [JsonPropertyName("regularMarketPrice")]
      public RegularMarketPrice RegularMarketPrice { get; set; }

      [JsonPropertyName("regularMarketDayHigh")]
      public RegularMarketDayHigh RegularMarketDayHigh { get; set; }

      [JsonPropertyName("regularMarketDayLow")]
      public RegularMarketDayLow RegularMarketDayLow { get; set; }

      [JsonPropertyName("regularMarketVolume")]
      public RegularMarketVolume RegularMarketVolume { get; set; }

      [JsonPropertyName("averageDailyVolume10Day")]
      public object AverageDailyVolume10Day { get; set; }

      [JsonPropertyName("averageDailyVolume3Month")]
      public object AverageDailyVolume3Month { get; set; }

      [JsonPropertyName("regularMarketPreviousClose")]
      public RegularMarketPreviousClose RegularMarketPreviousClose { get; set; }

      [JsonPropertyName("regularMarketSource")]
      public string RegularMarketSource { get; set; }

      [JsonPropertyName("regularMarketOpen")]
      public RegularMarketOpen RegularMarketOpen { get; set; }

      [JsonPropertyName("strikePrice")]
      public object StrikePrice { get; set; }

      [JsonPropertyName("openInterest")]
      public object OpenInterest { get; set; }

      [JsonPropertyName("exchange")]
      public string Exchange { get; set; }

      [JsonPropertyName("exchangeName")]
      public string ExchangeName { get; set; }

      [JsonPropertyName("exchangeDataDelayedBy")]
      public int ExchangeDataDelayedBy { get; set; }

      [JsonPropertyName("marketState")]
      public string MarketState { get; set; }

      [JsonPropertyName("quoteType")]
      public string QuoteType { get; set; }

      [JsonPropertyName("symbol")]
      public string Symbol { get; set; }

      [JsonPropertyName("underlyingSymbol")]
      public object UnderlyingSymbol { get; set; }

      [JsonPropertyName("shortName")]
      public string ShortName { get; set; }

      [JsonPropertyName("longName")]
      public string LongName { get; set; }

      [JsonPropertyName("currency")]
      public string Currency { get; set; }

      [JsonPropertyName("quoteSourceName")]
      public string QuoteSourceName { get; set; }

      [JsonPropertyName("currencySymbol")]
      public string CurrencySymbol { get; set; }

      [JsonPropertyName("fromCurrency")]
      public object FromCurrency { get; set; }

      [JsonPropertyName("toCurrency")]
      public object ToCurrency { get; set; }

      [JsonPropertyName("lastMarket")]
      public object LastMarket { get; set; }

      [JsonPropertyName("volume24Hr")]
      public object Volume24Hr { get; set; }

      [JsonPropertyName("volumeAllCurrencies")]
      public object VolumeAllCurrencies { get; set; }

      [JsonPropertyName("circulatingSupply")]
      public object CirculatingSupply { get; set; }

      [JsonPropertyName("marketCap")]
      public MarketCap MarketCap { get; set; }
    }

    public class PriceResult
    {
      [JsonPropertyName("price")]
      public Price Price { get; set; }
    }

    public class QuoteSummaryResult
    {
      [JsonPropertyName("result")]
      public List<PriceResult> Result { get; set; }

      [JsonPropertyName("error")]
      public object Error { get; set; }
    }

    public class QuoteSummary
    {
      [JsonPropertyName("quoteSummary")]
      public QuoteSummaryResult QuoteSummaryResult { get; set; }
    }
  }



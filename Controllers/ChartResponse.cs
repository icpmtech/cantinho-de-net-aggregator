using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MarketAnalyticHub.Models.YahooFinance
{
  // Root object
  public class ChartResponse
  {
    [JsonProperty("chart")]
    public Chart Chart { get; set; }
  }

  public class Chart
  {
    [JsonProperty("result")]
    public List<Result> Result { get; set; }

    [JsonProperty("error")]
    public object Error { get; set; }
  }

  public class Result
  {
    [JsonProperty("meta")]
    public Meta Meta { get; set; }

    [JsonProperty("indicators")]
    public Indicators Indicators { get; set; }
  }

  public class Meta
  {
    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("exchangeName")]
    public string ExchangeName { get; set; }

    [JsonProperty("fullExchangeName")]
    public string FullExchangeName { get; set; }

    [JsonProperty("instrumentType")]
    public string InstrumentType { get; set; }

    [JsonProperty("firstTradeDate")]
    public long FirstTradeDate { get; set; }

    [JsonProperty("regularMarketTime")]
    public long RegularMarketTime { get; set; }

    [JsonProperty("hasPrePostMarketData")]
    public bool HasPrePostMarketData { get; set; }

    [JsonProperty("gmtoffset")]
    public int GmtOffset { get; set; }

    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("exchangeTimezoneName")]
    public string ExchangeTimezoneName { get; set; }

    [JsonProperty("regularMarketPrice")]
    public decimal RegularMarketPrice { get; set; }

    [JsonProperty("fiftyTwoWeekHigh")]
    public decimal FiftyTwoWeekHigh { get; set; }

    [JsonProperty("fiftyTwoWeekLow")]
    public decimal FiftyTwoWeekLow { get; set; }

    [JsonProperty("regularMarketDayHigh")]
    public decimal RegularMarketDayHigh { get; set; }

    [JsonProperty("regularMarketDayLow")]
    public decimal RegularMarketDayLow { get; set; }

    [JsonProperty("regularMarketVolume")]
    public long RegularMarketVolume { get; set; }

    [JsonProperty("longName")]
    public string LongName { get; set; }

    [JsonProperty("shortName")]
    public string ShortName { get; set; }

    [JsonProperty("chartPreviousClose")]
    public decimal ChartPreviousClose { get; set; }

    [JsonProperty("priceHint")]
    public int PriceHint { get; set; }

    [JsonProperty("currentTradingPeriod")]
    public CurrentTradingPeriod CurrentTradingPeriod { get; set; }

    [JsonProperty("dataGranularity")]
    public string DataGranularity { get; set; }

    [JsonProperty("range")]
    public string Range { get; set; }

    [JsonProperty("validRanges")]
    public List<string> ValidRanges { get; set; }
  }

  public class CurrentTradingPeriod
  {
    [JsonProperty("pre")]
    public TradingPeriod Pre { get; set; }

    [JsonProperty("regular")]
    public TradingPeriod Regular { get; set; }

    [JsonProperty("post")]
    public TradingPeriod Post { get; set; }
  }

  public class TradingPeriod
  {
    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("start")]
    public long Start { get; set; }

    [JsonProperty("end")]
    public long End { get; set; }

    [JsonProperty("gmtoffset")]
    public int GmtOffset { get; set; }
  }

  public class Indicators
  {
    [JsonProperty("quote")]
    public List<Quote> Quote { get; set; }

    [JsonProperty("adjclose")]
    public List<AdjClose> Adjclose { get; set; }
  }

  public class Quote
  {
    [JsonProperty("open")]
    public List<decimal?> Open { get; set; }

    [JsonProperty("high")]
    public List<decimal?> High { get; set; }

    [JsonProperty("low")]
    public List<decimal?> Low { get; set; }

    [JsonProperty("close")]
    public List<decimal?> Close { get; set; }

    [JsonProperty("volume")]
    public List<long?> Volume { get; set; }
  }

  public class AdjClose
  {
    [JsonProperty("adjclose")]
    public List<decimal?> Adjclose { get; set; }
  }

  // Custom model for your application
  public class HistoricalData
  {
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
  }

  // Custom model to structure the response
  public class DataResult
  {
    public string[] Dates { get; set; }
    public decimal[] Opens { get; set; }
    public decimal[] Highs { get; set; }
    public decimal[] Lows { get; set; }
    public decimal[] Closes { get; set; }
    public decimal[] Volumes { get; set; }
  }
}

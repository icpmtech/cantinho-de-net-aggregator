namespace MarketAnalyticHub.Models.ViewsModels
{
  public class PortfolioDetailViewModel
  {
    public PortfolioDetailViewModel()
    {
      HistoricalData = new List<ChartDataPortfolioDetailViewModel>();
      RecentNews = new List<NewsItem>();
      CompanyOverview = "No company overview available.";
      ShortName = "N/A";
      LongName = "N/A";
      Symbol = "N/A";
      RegularMarketPrice = 0;
      RegularMarketChange = 0;
      RegularMarketChangePercent = 0;
      MarketCap = 0;
      TrailingPE = 0;
      ForwardPE = 0;
      DividendYield = 0;
      FiftyTwoWeekRange = "N/A";
      Revenue = 0;
      NetIncome = 0;
      EBITDA = 0;
      Id = 0;
    }

    public string ShortName { get; set; }
    public string LongName { get; set; }
    public string Symbol { get; set; }
    public double RegularMarketPrice { get; set; }
    public double RegularMarketChange { get; set; }
    public double RegularMarketChangePercent { get; set; }
    public long MarketCap { get; set; }
    public double TrailingPE { get; set; }
    public double ForwardPE { get; set; }
    public double DividendYield { get; set; }
    public string FiftyTwoWeekRange { get; set; }
    public List<ChartDataPortfolioDetailViewModel> HistoricalData { get; set; }
    public double Revenue { get; set; }
    public double NetIncome { get; set; }
    public double EBITDA { get; set; }
    public List<NewsItem> RecentNews { get; set; }
    public string CompanyOverview { get; set; }
    public int Id { get; set; } // For action links
    public string Language { get; set; }
    public string Region { get; set; }
    public string QuoteType { get; set; }
    public string TypeDisp { get; set; }
    public string QuoteSourceName { get; set; }
    public bool Triggerable { get; set; }
    public string CustomPriceAlertConfidence { get; set; }
    public string MarketState { get; set; }
    public string ExchangeTimezoneShortName { get; set; }
    public int GmtOffSetMilliseconds { get; set; }
    public bool EsgPopulated { get; set; }
    public string Currency { get; set; }
    public string Exchange { get; set; }
    public string MessageBoardId { get; set; }
    public string ExchangeTimezoneName { get; set; }
    public string Market { get; set; }
    public bool HasPrePostMarketData { get; set; }
    public long FirstTradeDateMilliseconds { get; set; }
    public double PostMarketPrice { get; set; }
    public double PostMarketChange { get; set; }
    public long RegularMarketTime { get; set; }
    public double RegularMarketDayHigh { get; set; }
    public string RegularMarketDayRange { get; set; }
    public double RegularMarketDayLow { get; set; }
    public int RegularMarketVolume { get; set; }
    public double RegularMarketPreviousClose { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public int BidSize { get; set; }
    public int AskSize { get; set; }
    public string FullExchangeName { get; set; }
    public string FinancialCurrency { get; set; }
    public double RegularMarketOpen { get; set; }
    public int AverageDailyVolume3Month { get; set; }
    public int AverageDailyVolume10Day { get; set; }
    public double FiftyTwoWeekLowChange { get; set; }
    public double FiftyTwoWeekLowChangePercent { get; set; }
    public double FiftyTwoWeekHighChange { get; set; }
    public double FiftyTwoWeekHighChangePercent { get; set; }
    public double FiftyTwoWeekLow { get; set; }
    public double FiftyTwoWeekHigh { get; set; }
    public double FiftyTwoWeekChangePercent { get; set; }
    public long EarningsTimestamp { get; set; }
    public long EarningsTimestampStart { get; set; }
    public long EarningsTimestampEnd { get; set; }
    public long EarningsCallTimestampStart { get; set; }
    public long EarningsCallTimestampEnd { get; set; }
    public bool IsEarningsDateEstimate { get; set; }
    public double TrailingAnnualDividendRate { get; set; }
    public double TrailingAnnualDividendYield { get; set; }
    public double EpsTrailingTwelveMonths { get; set; }
    public double EpsForward { get; set; }
    public double EpsCurrentYear { get; set; }
    public double PriceEpsCurrentYear { get; set; }
    public long SharesOutstanding { get; set; }
    public double BookValue { get; set; }
    public double FiftyDayAverage { get; set; }
    public double FiftyDayAverageChange { get; set; }
    public double FiftyDayAverageChangePercent { get; set; }
    public double TwoHundredDayAverage { get; set; }
    public double TwoHundredDayAverageChange { get; set; }
    public double TwoHundredDayAverageChangePercent { get; set; }
    public double PriceToBook { get; set; }
    public int SourceInterval { get; set; }
    public int ExchangeDataDelayedBy { get; set; }
    public string AverageAnalystRating { get; set; }
    public bool Tradeable { get; set; }
    public bool CryptoTradeable { get; set; }
    public int PriceHint { get; set; }
    public double PostMarketChangePercent { get; set; }
    public long PostMarketTime { get; set; }
    public string DisplayName { get; set; }
  }

  public class NewsItem
  {
    public NewsItem()
    {
      Date = DateTime.MinValue;
      Title = "No news available.";
    }

    public DateTime Date { get; set; }
    public string Title { get; set; }
  }

  public class ChartDataPortfolioDetailViewModel
  {
    public ChartDataPortfolioDetailViewModel()
    {
      Date = DateTime.MinValue;
      Value = 0;
    }

    public DateTime Date { get; set; }
    public double Value { get; set; }
  }
}

using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Dashboard;
using MarketAnalyticHub.Models.Portfolio;
using ClosedXML.Excel;
using MarketAnalyticHub.Controllers;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using AspnetCoreMvcFull.Services;
using Microsoft.OpenApi.Models;
using DocumentFormat.OpenXml.Vml;
using System.Globalization;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;

namespace MarketAnalyticHub.Services
{
  public class PortfolioService
  {
    private readonly ApplicationDbContext _context;
    private readonly FinnhubService _FinnhubService;
    private readonly ILogger<PortfolioService> _logger;
    private readonly IYahooFinanceService _yahooFinanceService;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;
    public PortfolioService(ApplicationDbContext context,   OpenAIService openAIService, FinnhubService finnhubService, IYahooFinanceService yahooFinanceService, ILogger<PortfolioService> logger, SentimentAnalysisService sentimentAnalysisService)
    {
      _context = context;
      _FinnhubService = finnhubService;
      _yahooFinanceService = yahooFinanceService;
      _logger = logger;
      _sentimentAnalysisService = sentimentAnalysisService;
      _openAIService = openAIService;
    }

    // Existing methods
    public async Task<PortfolioOveralStatsDto> GetTotalPortfolioOverall(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      decimal totalMarketValue = 0;
      decimal totalCustMarketValue = 0;
      decimal totalDifferenceValue = 0;

      foreach (var portfolio in portfolios)
      {
        var portfolioStats = CalculatePortfolioPercentages(portfolio);
        if (portfolioStats is not null)
        {
          totalMarketValue += portfolioStats.TotalMarketValue;
          totalCustMarketValue += portfolioStats.TotalCustMarketValue;
          totalDifferenceValue += portfolioStats.TotalDifferenceValue;
        }

      }

      decimal totalDifferencePercentage = (totalCustMarketValue != 0) ? (totalDifferenceValue / totalCustMarketValue) * 100 : 0;

      var overallStats = new PortfolioOveralStatsDto
      {
        TotalMarketValue = totalMarketValue,
        TotalCustMarketValue = totalCustMarketValue,
        TotalDifferenceValue = totalDifferenceValue,
        TotalDifferencePercentage = Math.Round(totalDifferencePercentage, 3)
      };
      return overallStats;
    }

    public PortfolioStatisticsDto GetPortfolioStatistics()
    {
      // Fetch and calculate portfolio statistics
      var totalValue = 42820.00;
      var totalSymbols = 8258;
      var items = new List<PortfolioItemDto>
            {
                new PortfolioItemDto { Category = "Electronic", Description = "Mobile, Earbuds, TV", Value = 82500.00 },
                new PortfolioItemDto { Category = "Fashion", Description = "T-shirt, Jeans, Shoes", Value = 23800.00 },
                new PortfolioItemDto { Category = "Decor", Description = "Fine Art, Dining", Value = 849000.00 },
                new PortfolioItemDto { Category = "Sports", Description = "Football, Cricket Kit", Value = 9900.00 }
            };
      var chartData = new ChartDataDto
      {
        Labels = items.Select(i => i.Category).ToList(),
        Values = items.Select(i => i.Value).ToList()
      };

      return new PortfolioStatisticsDto
      {
        TotalValue = totalValue,
        TotalSymbols = totalSymbols,
        Items = items,
        ChartData = chartData
      };
    }

    public async Task<DashboardData> GetDashboardDataAsync(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      decimal totalInvestment = portfolios.Sum(p => p.TotalInvestment);
      decimal currentMarketValue = portfolios.Sum(p => p.CurrentMarketValue);
      decimal dividends = portfolios.Sum(p => p.Items.Sum(i => i.Dividends.Sum(d => d.Amount)));
      decimal profit = currentMarketValue - totalInvestment;
      decimal payments = 2456; // This should be replaced with actual payments data
      decimal operations = 14857; // This should be replaced with actual operations data
      decimal yearlyReport = 84686; // This should be replaced with actual yearly report data
      decimal growth = totalInvestment > 0 ? (profit / totalInvestment) * 100 : 0;
      decimal portfolioGrowth = totalInvestment > 0 ? (currentMarketValue / totalInvestment) * 100 : 0;

      return new DashboardData
      {
        Profit = profit,
        Dividends = dividends,
        Payments = payments,
        Operations = operations,
        TotalRevenue = currentMarketValue, // Example data
        Growth = growth,
        PortfolioGrowth = portfolioGrowth,
        YearlyReport = yearlyReport
      };
    }
    public async Task<IEnumerable<PurchaseData>> GetPurchaseDatesForSymbol(string userId, string symbol, DateTime startDate, DateTime endDate)
    {
      var portfolios = await _context.Portfolios
                                    .Include(p => p.Items)
                                    .ThenInclude(pi => pi.Dividends)
                                    .Include(p => p.Items)
                                    .ThenInclude(pi => pi.StockEvents)
                                    .Where(p => p.UserId == userId)
                                    .ToListAsync();

      var purchaseData = portfolios
          .SelectMany(p => p.Items)
          .Where(pi => pi.Symbol == symbol && pi.PurchaseDate >= startDate && pi.PurchaseDate <= endDate)
          .Select(pi => new PurchaseData
          {
            OperationType=pi.OperationType,
            Date = pi.PurchaseDate,
            Quantity = pi.Quantity
          });

      return purchaseData;
    }

    public class PurchaseData
    {
      public DateTime Date { get; set; }
      public int Quantity { get; set; }
      public string OperationType { get; internal set; }
    }
    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      var portfolios = await _context.Portfolios
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.Dividends)
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.StockEvents)
                                     .Where(p => p.UserId == userId)
                                     .ToListAsync();

      // Update current prices and calculate fields
      // Create a dictionary to store stock data by symbol
      var symbolStockData = new Dictionary<string, StockData>();

      // Fetch the real-time price for each unique symbol once
      foreach (var symbol in portfolios.SelectMany(p => p.Items).Select(i => i.Symbol).Distinct())
      {
        var stockData = await GetCurrentPriceAsync(symbol);
        symbolStockData[symbol] = stockData;
      }

      // Update the portfolio items with the fetched stock data
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          var stockData = symbolStockData[item.Symbol];
          item.CurrentPrice = (decimal)stockData.CurrentPrice;
          item.Change = (decimal)stockData.Change;
          item.PercentChange = (decimal)stockData.PercentChange;
          item.HighPrice = (decimal)stockData.HighPrice;
          item.LowPrice = (decimal)stockData.LowPrice;
          item.OpenPrice = (decimal)stockData.OpenPrice;
          item.PreviousClosePrice = (decimal)stockData.PreviousClosePrice;
        }
      }

      return portfolios;
    }

    public async Task<Portfolio> GetPortfolioByIdAsync(int id)
    {
      var portfolio = await _context.Portfolios
                                    .Include(p => p.Items)
                                    .ThenInclude(pi => pi.Dividends)
                                    .FirstOrDefaultAsync(p => p.Id == id);

      if (portfolio != null)
      {
        foreach (var item in portfolio.Items)
        {
          var stockData = await GetCurrentPriceAsync(item.Symbol);
          item.CurrentPrice = (decimal)stockData.CurrentPrice;
          item.Change = (decimal)stockData.Change;
          item.PercentChange = (decimal)stockData.PercentChange;
          item.HighPrice = (decimal)stockData.HighPrice;
          item.LowPrice = (decimal)stockData.LowPrice;
          item.OpenPrice = (decimal)stockData.OpenPrice;
          item.PreviousClosePrice = (decimal)stockData.PreviousClosePrice;
        }
      }

      return portfolio;
    }

    private async Task<StockData> GetCurrentPriceAsync(string symbol)
    {
      try
      {
        var stockData = await _yahooFinanceService.GetRealTimePriceAsync(symbol);
        return stockData;
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        _logger.LogError(ex, $"Failed to get current price for symbol: {symbol}");
        return new StockData
        {
          CurrentPrice = 0,
          Change = 0,
          PercentChange = 0,
          HighPrice = 0,
          LowPrice = 0,
          OpenPrice = 0,
          PreviousClosePrice = 0
        };
      }
    }

    public async Task AddPortfolioAsync(Portfolio portfolio)
    {
      _context.Portfolios.Add(portfolio);
      await _context.SaveChangesAsync();
    }

    public async Task UpdatePortfolioAsync(Portfolio portfolio)
    {
      _context.Portfolios.Update(portfolio);
      await _context.SaveChangesAsync();
    }

    public async Task DeletePortfolioAsync(int id)
    {
      var portfolio = await _context.Portfolios.FindAsync(id);
      _context.Portfolios.Remove(portfolio);
      await _context.SaveChangesAsync();
    }
    public async Task DeletePortfolioAllAsync(string userId)
    {
      var portfolios = await _context.Portfolios.Where(p => p.UserId == userId).ToListAsync();
      foreach (var portfolio in portfolios)
      {
        _context.Portfolios.Remove(portfolio);
        await _context.SaveChangesAsync();
      }

    }
    public async Task<IEnumerable<Portfolio>> GetPortfoliosAsync()
    {
      return await _context.Portfolios.Include(p => p.Items).ToListAsync();
    }

    public string ExportToCsv(IEnumerable<Portfolio> portfolios)
    {
      var csvBuilder = new StringBuilder();
      csvBuilder.AppendLine("PortfolioName,ItemSymbol,ItemQuantity,ItemPurchasePrice,ItemPurchaseDate");

      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          csvBuilder.AppendLine($"{portfolio.Name},{item.Symbol},{item.Quantity},{item.PurchasePrice},{item.PurchaseDate}");
        }
      }

      return csvBuilder.ToString();
    }

    public XLWorkbook ExportToExcel(IEnumerable<Portfolio> portfolios)
    {
      var workbook = new XLWorkbook();
      var worksheet = workbook.Worksheets.Add("Portfolios");

      worksheet.Cell(1, 1).Value = "PortfolioName";
      worksheet.Cell(1, 2).Value = "ItemSymbol";
      worksheet.Cell(1, 3).Value = "ItemQuantity";
      worksheet.Cell(1, 4).Value = "ItemPurchasePrice";
      worksheet.Cell(1, 5).Value = "ItemPurchaseDate";

      var currentRow = 2;
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          worksheet.Cell(currentRow, 1).Value = portfolio.Name;
          worksheet.Cell(currentRow, 2).Value = item.Symbol;
          worksheet.Cell(currentRow, 3).Value = item.Quantity;
          worksheet.Cell(currentRow, 4).Value = item.PurchasePrice;
          worksheet.Cell(currentRow, 5).Value = item.PurchaseDate;
          currentRow++;
        }
      }

      return workbook;
    }
  
    public async Task ImportYahooFromCsv(string csvData, string userId)
    {
      var lines = csvData.Split('\n').Skip(1); // Skip header line

      var portfolioName = "Import_Yahoo";
      var portfolio = await _context.Portfolios.Include(p => p.Items)
                                               .FirstOrDefaultAsync(p => p.Name == portfolioName && p.UserId == userId);

      if (portfolio == null)
      {
        portfolio = new Portfolio { Name = portfolioName, UserId = userId, Items = new List<PortfolioItem>() };
        _context.Portfolios.Add(portfolio);
      }

      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line)) continue;

        var parts = line.Split(',');
        if (parts.Length < 5) continue; // Skip incomplete lines

        var itemSymbol = parts[0].Trim();
        var itemQuantity = Converter.ConvertToInt(parts[11].Trim());
        var itemPurchasePrice = Converter.ConvertToDecimal(parts[10].Trim());
        var itemPurchaseDate = Converter.ConvertToDateTime(parts[9].Trim());

        portfolio.Items.Add(new PortfolioItem
        {
          Symbol = itemSymbol,
          Quantity = itemQuantity,
          PurchasePrice = itemPurchasePrice,
          PurchaseDate = itemPurchaseDate,
          UserId = userId,
          OperationType = "N/A"
        });
      }

      await _context.SaveChangesAsync();
    }

    public async Task ImportYahooFromExcel(Stream stream, string userId)
    {
      using (var workbook = new XLWorkbook(stream))
      {
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1); // Skip header row

        foreach (var row in rows)
        {
          var portfolioName = row.Cell(1).GetString().Trim();
          var itemSymbol = row.Cell(2).GetString().Trim();
          var itemQuantity = int.TryParse(row.Cell(3).GetString().Trim(), out int quantity) ? quantity : 0;
          var itemPurchasePrice = decimal.TryParse(row.Cell(4).GetString().Trim(), out decimal purchasePrice) ? purchasePrice : 0m;
          var itemPurchaseDate = DateTime.TryParse(row.Cell(5).GetString().Trim(), out DateTime purchaseDate) ? purchaseDate : DateTime.MinValue;

          var portfolio = await _context.Portfolios.Include(p => p.Items)
                                                   .FirstOrDefaultAsync(p => p.Name == portfolioName && p.UserId == userId);

          if (portfolio == null)
          {
            portfolio = new Portfolio { Name = portfolioName, UserId = userId, Items = new List<PortfolioItem>() };
            _context.Portfolios.Add(portfolio);
          }

          portfolio.Items.Add(new PortfolioItem
          {
            Symbol = itemSymbol,
            Quantity = itemQuantity,
            PurchasePrice = itemPurchasePrice,
            PurchaseDate = itemPurchaseDate,
            UserId = userId,
            OperationType="N/A"
          });
        }

        await _context.SaveChangesAsync();
      }
    }
  
  public async Task ImportFromCsv(string csvData, string userId)
    {
      var lines = csvData.Split('\n').Skip(1); // Skip header line

      foreach (var line in lines)
      {
        if (string.IsNullOrWhiteSpace(line)) continue;

        var parts = line.Split(',');
        var portfolioName = parts[0].Trim();
        var itemSymbol = parts[1].Trim();
        var itemQuantity = int.Parse(parts[2].Trim());
        var itemPurchasePrice = decimal.Parse(parts[3].Trim());
        var itemPurchaseDate = DateTime.Parse(parts[4].Trim());

        var portfolio = await _context.Portfolios.Include(p => p.Items)
                                                 .FirstOrDefaultAsync(p => p.Name == portfolioName && p.UserId == userId);

        if (portfolio == null)
        {
          portfolio = new Portfolio { Name = portfolioName, UserId = userId, Items = new List<PortfolioItem>() };
          _context.Portfolios.Add(portfolio);
        }

        portfolio.Items.Add(new PortfolioItem
        {
          Symbol = itemSymbol,
          Quantity = itemQuantity,
          PurchasePrice = itemPurchasePrice,
          PurchaseDate = itemPurchaseDate
        });
      }

      await _context.SaveChangesAsync();
    }

    public async Task ImportFromExcel(Stream stream, string userId)
    {
      using (var workbook = new XLWorkbook(stream))
      {
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1); // Skip header row

        foreach (var row in rows)
        {
          var portfolioName = row.Cell(1).GetString().Trim();
          var itemSymbol = row.Cell(2).GetString().Trim();
          var itemQuantity = row.Cell(3).GetValue<int>();
          var itemPurchasePrice = row.Cell(4).GetValue<decimal>();
          var itemPurchaseDate = row.Cell(5).GetDateTime();

          var portfolio = await _context.Portfolios.Include(p => p.Items)
                                                   .FirstOrDefaultAsync(p => p.Name == portfolioName && p.UserId == userId);

          if (portfolio == null)
          {
            portfolio = new Portfolio { Name = portfolioName, UserId = userId, Items = new List<PortfolioItem>() };
            _context.Portfolios.Add(portfolio);
          }

          portfolio.Items.Add(new PortfolioItem
          {
            Symbol = itemSymbol,
            Quantity = itemQuantity,
            PurchasePrice = itemPurchasePrice,
            PurchaseDate = itemPurchaseDate
          });
        }

        await _context.SaveChangesAsync();
      }
    }

    public async Task<List<CandlestickData>> GetCandlestickDataAsync(string symbol, string resolution, int count)
    {
      try
      {
        return await _FinnhubService.GetCandlestickDataAsync(symbol, resolution, count);
      }
      catch (Exception ex)
      {
        // Log the exception
        _logger.LogError(ex, $"Failed to get candlestick data for symbol: {symbol}");

        // Return an empty list or handle the error as needed
        return new List<CandlestickData>();
      }
    }

    // New Method to Calculate Original Market Value
    public double CalculateOriginalMarketValue(double currentMarketValue, double percentageIncrease)
    {
      return currentMarketValue / (1 + (percentageIncrease / 100));
    }

    // New Method to Calculate Percentage Change
    public double CalculatePercentageChange(double originalMarketValue, double currentMarketValue)
    {
      return ((currentMarketValue - originalMarketValue) / originalMarketValue) * 100;
    }

    // New Method to Calculate Portfolio Item Percentages
    public PortfolioPercentageResponse CalculatePortfolioPercentages(Portfolio portfolio)
    {
      if (portfolio == null || portfolio.Items == null || !portfolio.Items.Any())
        return null;

      // Create a dictionary to store the real-time prices
      var symbolPrices = new Dictionary<string, decimal>();

      // Fetch the real-time price for each unique symbol once
      foreach (var symbol in portfolio.Items.Select(item => item.Symbol).Distinct())
      {
        var realTimePrice = _yahooFinanceService.GetRealTimePriceAsync(symbol).Result.CurrentPrice;
        symbolPrices[symbol] = (decimal)realTimePrice;
      }

      // Calculate total customer market value (initial investment cost including commissions)
      var totalCustMarketValue = portfolio.Items.Sum(item => item.PurchasePrice * item.Quantity + (item.Commission ?? 0));

      // Calculate total dividends
      var totalDividends = portfolio.Items.Sum(item => (item.Dividends?.Sum(s => s.Amount)) ?? 0);

      // Calculate total market value (current market value without dividends)
      var totalMarketValue = portfolio.Items.Sum(item => symbolPrices[item.Symbol] * item.Quantity);

      // Calculate total market value with dividends
      var totalWithDividendsMarketValue = totalMarketValue + totalDividends;

      // Calculate total difference in value and percentage
      var totalDifferenceValue = totalMarketValue - totalCustMarketValue;
      var totalDifferencePercentage = (totalCustMarketValue != 0) ? (totalDifferenceValue / totalCustMarketValue) * 100 : 0;

      // Calculate total difference in value dividends and percentage
      var totalDifferenceDividendsValue = totalWithDividendsMarketValue - totalCustMarketValue;
      var totalDifferenceDividendsPercentage = (totalCustMarketValue != 0) ? (totalDifferenceDividendsValue / totalCustMarketValue) * 100 : 0;

      // Calculate item percentages
      var itemPercentages = portfolio.Items.Select(item =>
      {
        var currentPrice = symbolPrices[item.Symbol];
        var currentMarketValue = currentPrice * item.Quantity;
        var customMarketValue = item.PurchasePrice * item.Quantity + (item.Commission ?? 0);
        var differenceValue = currentMarketValue - customMarketValue;
        var differencePercentage = (customMarketValue != 0) ? (differenceValue / customMarketValue) * 100 : 0;
        var itemDividends = (item.Dividends?.Sum(s => s.Amount)) ?? 0;
        var currentMarketValueWithDividends = currentMarketValue + itemDividends;

        return new PortfolioItemPercentage
        {
          Symbol = item.Symbol,
          CurrentPercentage = (totalMarketValue != 0) ? (currentMarketValue / totalMarketValue) * 100 : 0,
          CustomPercentage = (totalCustMarketValue != 0) ? (customMarketValue / totalCustMarketValue) * 100 : 0,
          CurrentWithDividendsPercentage = (totalWithDividendsMarketValue != 0) ? (currentMarketValueWithDividends / totalWithDividendsMarketValue) * 100 : 0,
          CustomWithDividendsPercentage = (totalCustMarketValue != 0) ? (customMarketValue / totalCustMarketValue) * 100 : 0,
          DifferenceValue = differenceValue,
          DifferencePercentage = differencePercentage,
          DifferenceWithDividendsValue = currentMarketValueWithDividends - customMarketValue,
          DifferenceWithDividendsPercentage = (customMarketValue != 0) ? ((currentMarketValueWithDividends - customMarketValue) / customMarketValue) * 100 : 0
        };
      }).ToList();

      // Return the portfolio percentage response
      return new PortfolioPercentageResponse
      {
        PortfolioId = portfolio.Id,
        TotalMarketValue = totalMarketValue,
        TotalCustMarketValue = totalCustMarketValue,
        TotalPortfolioProfit = totalDifferenceDividendsValue,
        TotalDifferenceValue = totalDifferenceValue,
        TotalDifferenceWithDividendsPercentage = totalDifferenceDividendsPercentage,
        TotalDifferencePercentage = totalDifferencePercentage,
        ItemPercentages = itemPercentages
      };
    }


    // New Method to Map Sentiment to Portfolio
    public async Task<PortfolioSentimentImpactResponse> MapSentimentToPortfolioAsync(string userId, string newsText)
    {
      // Step 1: Analyze sentiment
      var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsText);

      // Step 2: Get associated symbols (keywords) from sentiment analysis (assuming this functionality exists)
      var keywords = await _openAIService.GenerateKeywordsAsync(newsText);

      // Step 3: Get portfolios for the user
      var portfolios = await GetPortfoliosByUserAsync(userId);

      // Step 4: Map sentiment to relevant portfolio items
      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          if (keywords.Contains(item.Symbol))
          {
            // Assuming the sentiment score is between -1 (negative) to 1 (positive)
            item.SentimentImpact = sentimentResult.Score;
            item.AdjustedPrice = item.CurrentPrice * (decimal?)(1 + item.SentimentImpact);
          }
        }
      }

      // Step 5: Calculate the overall impact on the portfolio
      var totalMarketValueBeforeImpact = portfolios.Sum(p => p.Items.Sum(i => i.CurrentPrice * i.Quantity));
      var totalMarketValueAfterImpact = portfolios.Sum(p => p.Items.Sum(i => (i.AdjustedPrice ?? i.CurrentPrice) * i.Quantity));

      return new PortfolioSentimentImpactResponse
      {
        TotalMarketValueBeforeImpact = (double)totalMarketValueBeforeImpact,
        TotalMarketValueAfterImpact = (double)totalMarketValueAfterImpact,
        PortfolioItems = portfolios.SelectMany(p => p.Items).ToList()
      };
    }
  }

  // New DTOs
  public class PortfolioSentimentImpactResponse
  {
    public double TotalMarketValueBeforeImpact { get; set; }
    public double TotalMarketValueAfterImpact { get; set; }
    public List<Models.Portfolio.PortfolioItem> PortfolioItems { get; set; }
  }

}

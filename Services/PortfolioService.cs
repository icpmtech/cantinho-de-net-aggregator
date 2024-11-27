using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Dashboard;
using MarketAnalyticHub.Models.Portfolio;
using ClosedXML.Excel;
using MarketAnalyticHub.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models.Portfolio.Entities;
using Microsoft.AspNetCore.SignalR;
using MarketAnalyticHub.Controllers.AIPilot;
using AngleSharp.Text;
using MarketAnalyticHub.Controllers.api;
using MarketAnalyticHub.Repositories;

namespace MarketAnalyticHub.Services
{
  public class PortfolioService
  {
    private readonly ApplicationDbContext _context;
    private readonly IYahooFinanceService _yahooFinanceService;
    private readonly ILogger<PortfolioService> _logger;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;
    private readonly FinnhubService _FinnhubService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private object value;
    private object value1;
    private object value2;
    private readonly IDataRepository _dataRepository;
    private readonly IStockPriceService _stockPriceService;


    public PortfolioService(ApplicationDbContext context, IDataRepository dataRepository, IStockPriceService stockPriceService, IHubContext<NotificationHub> hubContext, FinnhubService finnhubService, OpenAIService openAIService, IYahooFinanceService yahooFinanceService, ILogger<PortfolioService> logger, SentimentAnalysisService sentimentAnalysisService)
    {
      _context = context;
      _dataRepository = dataRepository;
      _stockPriceService = stockPriceService;
      _yahooFinanceService = yahooFinanceService;
      _logger = logger;
      _sentimentAnalysisService = sentimentAnalysisService;
      _openAIService = openAIService;
      _FinnhubService = finnhubService;
      _hubContext = hubContext;
    }

    public PortfolioService(object value)
    {
      this.value = value;
    }

    public PortfolioService(object value, object value1) : this(value)
    {
      this.value1 = value1;
    }

    public PortfolioService(object value, object value1, object value2) : this(value, value1)
    {
      this.value2 = value2;
    }

    // Method to trigger a portfolio update notification
    public async Task UpdatePortfolioValueAsync(string userId, decimal newValue)
    {
      string message = $"Your portfolio value has been updated to {newValue:C}";
      await _hubContext.Clients.User(userId).SendAsync("ReceivePortfolioUpdate", message);
    }

    // Method to trigger a stock alert notification
    public async Task SendStockAlertAsync(string userId, string stockSymbol, decimal currentPrice)
    {
      string message = $"Alert: {stockSymbol} is now trading at {currentPrice:C}";
      await _hubContext.Clients.User(userId).SendAsync("ReceiveStockAlert", message);
    }
    public async Task<PortfolioOveralStatsDto> GetTotalPortfolioOverall(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      if (portfolios == null || !portfolios.Any())
      {
        return new PortfolioOveralStatsDto
        {
          TotalMarketValue = 0,
          TotalCustMarketValue = 0,
          TotalDifferenceValue = 0,
          TotalDifferencePercentage = 0,
          TotalPortfolioProfit = 0,
          TotalDifferenceWithDividendsPercentage = 0,
          TotalDividendsPercentage = 0,
          TotalDividends = 0,
          ItemPercentages = new List<PortfolioItemPercentage>()
        };
      }

      decimal totalMarketValue = 0;
      decimal totalCustMarketValue = 0;
      decimal totalDifferenceValue = 0;
      decimal totalDifferencePercentage = 0;
      decimal totalPortfolioProfit = 0;
      decimal totalDifferenceWithDividendsPercentage = 0;
      decimal totalDividendsPercentage = 0;
      decimal totalDividends = 0;
      var itemPercentages = new List<PortfolioItemPercentage>();

      foreach (var portfolio in portfolios)
      {
        var portfolioPercentageResponse = await CalculateTotalPortfolioPercentagesAsync(portfolio);
        if (portfolioPercentageResponse != null)
        {
          totalMarketValue += portfolioPercentageResponse.TotalMarketValue;
          totalCustMarketValue += portfolioPercentageResponse.TotalCustMarketValue;
          totalDifferenceValue += portfolioPercentageResponse.TotalDifferenceValue;
          totalPortfolioProfit += portfolioPercentageResponse.TotalPortfolioProfit;
          totalDifferenceWithDividendsPercentage += portfolioPercentageResponse.TotalDifferenceWithDividendsPercentage;
          totalDividendsPercentage += portfolioPercentageResponse.TotalDividendsPercentage;
          totalDividends += portfolioPercentageResponse.TotalDividends;

          // Ensure ItemPercentages is not null before adding
          if (portfolioPercentageResponse.ItemPercentages != null)
          {
            itemPercentages.AddRange(portfolioPercentageResponse.ItemPercentages);
          }
        }
      }

      if (totalCustMarketValue != 0)
      {
        totalDifferencePercentage = (totalDifferenceValue / totalCustMarketValue) * 100;
      }

      return new PortfolioOveralStatsDto
      {
        TotalMarketValue = totalMarketValue,
        TotalCustMarketValue = totalCustMarketValue,
        TotalDifferenceValue = totalDifferenceValue,
        TotalDifferencePercentage = Math.Round(totalDifferencePercentage, 3),
        TotalPortfolioProfit = totalPortfolioProfit,
        TotalDifferenceWithDividendsPercentage = Math.Round(totalDifferenceWithDividendsPercentage, 3),
        TotalDividendsPercentage = totalDividendsPercentage,
        TotalDividends = totalDividends,
        ItemPercentages = itemPercentages
      };
    }


    public PortfolioStatisticsDto GetPortfolioStatistics()
    {
      // Dummy data for illustration
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
      int? payments = portfolios.Select(x => x.Items.Sum(st => st.StockEvents?.Count() ?? 0)).Sum(); // This should be replaced with actual payments data
      int? operations = portfolios.Sum(p => p.Items.Count()); // This should be replaced with actual operations data
      decimal yearlyReport = 0; // This should be replaced with actual yearly report data
      decimal growth = totalInvestment > 0 ? (profit / totalInvestment) * 100 : 0;
      decimal portfolioGrowth = totalInvestment > 0 ? (currentMarketValue / totalInvestment) * 100 : 0;

      return new DashboardData
      {
        Profit = profit,
        Dividends = dividends,
        Payments = payments,
        Operations = operations,
        TotalRevenue = currentMarketValue,
        Growth = growth,
        PortfolioGrowth = portfolioGrowth,
        YearlyReport = yearlyReport
      };
    }

    public async Task<IEnumerable<PurchaseData>> GetPurchaseDatesForSymbol(string userId, string symbol, DateTime startDate, DateTime endDate)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);

      var purchaseData = portfolios
          .SelectMany(p => p.Items)
          .Where(pi => pi.Symbol == symbol && pi.PurchaseDate >= startDate && pi.PurchaseDate <= endDate)
          .Select(pi => new PurchaseData
          {
            OperationType = pi.OperationType,
            Date = pi.PurchaseDate,
            Quantity = pi.Quantity
          });

      return purchaseData;
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

      var symbolStockData = await GetStockDataForPortfolios(portfolios);

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
    public async Task<IEnumerable<Portfolio>> GetPortfoliosByLossesUserAsync(string userId)
    {
      // Fetch portfolios with related data
      var portfolios = await _context.Portfolios
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.Dividends)
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.StockEvents)
                                     .Where(p => p.UserId == userId)
                                     .ToListAsync();

      // Fetch the latest stock data for the portfolios
      var symbolStockData = await GetStockDataForPortfolios(portfolios);

      foreach (var portfolio in portfolios)
      {
        decimal totalInitialValue = 0;
        decimal totalCurrentValue = 0;

        foreach (var item in portfolio.Items)
        {
          // Update portfolio items with the latest stock data
          var stockData = symbolStockData[item.Symbol];
          item.CurrentPrice = (decimal)stockData.CurrentPrice;
          item.Change = (decimal)stockData.Change;
          item.PercentChange = (decimal)stockData.PercentChange;
          item.HighPrice = (decimal)stockData.HighPrice;
          item.LowPrice = (decimal)stockData.LowPrice;
          item.OpenPrice = (decimal)stockData.OpenPrice;
          item.PreviousClosePrice = (decimal)stockData.PreviousClosePrice;

          // Calculate total initial value and total current value of the portfolio
          totalInitialValue += item.Quantity * item.PurchasePrice;
          totalCurrentValue += item.Quantity * item.CurrentPrice;
        }

        // Calculate the loss percentage
        if (totalInitialValue > 0)
        {
          var lossPercentage = ((totalInitialValue - totalCurrentValue) / totalInitialValue) * 100;
          portfolio.LossPercentage = lossPercentage;

          // If loss exceeds 2%, flag the portfolio (or take appropriate action)
          if (lossPercentage >= 2)
          {
            portfolio.IsLossAlertTriggered = true; // You can add this property to the Portfolio model
                                                   // Optionally, trigger an alert to the user here
          }
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
        var symbolStockData = await GetStockDataForPortfolioItems(portfolio.Items);

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

      return portfolio;
    }

    public async Task<decimal> GrowthPercentage(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);
      var portfolioItems = portfolios.SelectMany(p => p.Items).ToList();

      var symbols = portfolioItems.Select(item => item.Symbol).Distinct().ToList();
      var symbolToCurrentPrice = await GetCurrentPricesAsync(symbols);

      var totalCurrentMarketValue = portfolioItems
          .Sum(item => item.Quantity * symbolToCurrentPrice[item.Symbol].CurrentPrice);

      var totalInitialInvestment = portfolioItems
          .Sum(item => item.TotalInvestment);

      decimal growthPercentage = 0;
      if (totalInitialInvestment > 0)
      {
        growthPercentage = (((decimal)totalCurrentMarketValue - totalInitialInvestment) / totalInitialInvestment) * 100;
      }

      return growthPercentage;
    }

    public async Task<StockData> GetCurrentPriceAsync(string symbol)
    {
      try
      {
        var stockData = await _yahooFinanceService.GetRealTimePriceAsync(symbol);
        return stockData;
      }
      catch (Exception ex)
      {
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

    private async Task<Dictionary<string, StockData>> GetStockDataForPortfolioItems(IEnumerable<PortfolioItem> items)
    {
      var symbols = items.Select(i => i.Symbol).Distinct();
      var symbolStockData = new Dictionary<string, StockData>();

      foreach (var symbol in symbols)
      {
        var stockData = await GetCurrentPriceAsync(symbol);
        symbolStockData[symbol] = stockData;
      }

      return symbolStockData;
    }

    private async Task<Dictionary<string, StockData>> GetStockDataForPortfolios(IEnumerable<Portfolio> portfolios)
    {
      var symbols = portfolios.SelectMany(p => p.Items).Select(i => i.Symbol).Distinct();
      var symbolStockData = new Dictionary<string, StockData>();

      foreach (var symbol in symbols)
      {
        var stockData = await GetCurrentPriceAsync(symbol);
        symbolStockData[symbol] = stockData;
      }

      return symbolStockData;
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
      _context.Portfolios.RemoveRange(portfolios);
      await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Portfolio>> GetPortfoliosAsync()
    {
      return await _context.Portfolios.Include(p => p.Items).ToListAsync();
    }

    public string ExportToCsv(IEnumerable<Portfolio> portfolios)
    {
      var csvBuilder = new StringBuilder();
      csvBuilder.AppendLine("PortfolioName,ItemSymbol,ItemQuantity,ItemPurchasePrice,ItemPurchaseDate,Commission");

      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          csvBuilder.AppendLine($"{portfolio.Name},{item.Symbol},{item.Quantity},{item.PurchasePrice},{item.PurchaseDate},{item.Commission}");
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
        var itemCommission = Converter.ConvertToDecimal(parts[12].Trim());

        portfolio.Items.Add(new PortfolioItem
        {
          Symbol = itemSymbol,
          Quantity = itemQuantity,
          PurchasePrice = itemPurchasePrice,
          PurchaseDate = itemPurchaseDate,
          UserId = userId,
          OperationType = "Buy",
          Commission = itemCommission,
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
            OperationType = "Buy"
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
        _logger.LogError(ex, $"Failed to get candlestick data for symbol: {symbol}");
        return new List<CandlestickData>();
      }
    }

    public double CalculateOriginalMarketValue(double currentMarketValue, double percentageIncrease)
    {
      return currentMarketValue / (1 + (percentageIncrease / 100));
    }

    public double CalculatePercentageChange(double originalMarketValue, double currentMarketValue)
    {
      return ((currentMarketValue - originalMarketValue) / originalMarketValue) * 100;
    }
    public decimal CalculatePercentageChange(decimal originalMarketValue, decimal currentMarketValue)
    {
      return ((currentMarketValue - originalMarketValue) / originalMarketValue) * 100;
    }


    public async Task<List<TotalPortfolioPercentageResponse>> CalculatePortfolioPercentagesAsync(List<Portfolio> portfolios)
    {
      var portfolioPercentageResponses = new List<TotalPortfolioPercentageResponse>();

      foreach (var portfolio in portfolios)
      {
        var totalPortfolioPercentageResponse = await CalculateTotalPortfolioPercentagesAsync(portfolio);
        portfolioPercentageResponses.Add(totalPortfolioPercentageResponse);
      }

      return portfolioPercentageResponses;
    }

    public List<TotalRevenueByYearDto> GetTotalRevenueByYear(List<Portfolio> portfolios)
    {
      return portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => item.PurchaseDate.Year)
          .Select(g => new TotalRevenueByYearDto
          {
            Year = g.Key,
            TotalRevenue = g.Sum(item => item.CurrentMarketValue)
          })
          .ToList();
    }

    public async Task<decimal> CalculatePortfolioGrowthPercentage(string userId)
    {
      var portfolioGrowthPercentage = await GrowthPercentage(userId);
      return Math.Round(portfolioGrowthPercentage, 3);
    }

    public List<AmountTotalYearDto> GetAmountTotalYearByItems(List<Portfolio> portfolios)
    {
      return portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => item.PurchaseDate.Year)
          .Select(g => new AmountTotalYearDto
          {
            Year = g.Key,
            TotalInvestment = g.Sum(item => item.TotalInvestment)
          })
          .ToList();
    }

    public List<Portfolio> GetProfileReportCurrentYear(List<Portfolio> portfolios)
    {
      var currentYear = DateTime.Now.Year;
      return portfolios
          .Where(p => p.CreationDate.HasValue && p.CreationDate.Value.Year == currentYear)
          .ToList();
    }

    public List<TransactionDto> GetRecentTransactions(List<Portfolio> portfolios)
    {
      var fromDate = DateTime.Now.AddDays(-28);
      return portfolios
          .SelectMany(p => p.Items)
          .Where(item => item.PurchaseDate >= fromDate)
          .Select(item => new TransactionDto
          {
            Type = item.OperationType,
            Description = item.Symbol,
            Icon = PortfolioHelpers.GetIconForTransaction(item.Symbol),
            Amount = item.Quantity * item.PurchasePrice,
            Currency = "€",
            Date = item.PurchaseDate,
            Source = item.Symbol
          })
          .ToList();
    }

    public async Task<TotalPortfolioPercentageResponse> CalculateTotalPortfolioPercentagesAsync(Portfolio portfolio)
    {
      if (portfolio == null || portfolio.Items == null || !portfolio.Items.Any())
        return null;

      var symbolPrices = await GetCurrentPricesAsync(portfolio.Items.Select(item => item.Symbol).Distinct());

      var totalCustMarketValue = portfolio.Items.Sum(item => item.PurchasePrice * item.Quantity + (item.Commission ?? 0));
      var totalDividends = portfolio.Items.Sum(item => item.Dividends?.Sum(d => d.Amount) ?? 0);
      var totalMarketValue = portfolio.Items.Sum(item => symbolPrices[item.Symbol].CurrentPrice * item.Quantity);
      var totalWithDividendsMarketValue =(decimal) totalMarketValue + totalDividends;
      var totalDifferenceValue = (decimal)totalMarketValue - totalCustMarketValue;
      var totalDifferencePercentage = (totalCustMarketValue != 0) ? (totalDifferenceValue / totalCustMarketValue) * 100 : 0;
      var totalDifferenceDividendsValue = totalWithDividendsMarketValue - totalCustMarketValue;
      var totalDifferenceDividendsPercentage = (totalCustMarketValue != 0) ? (totalDifferenceDividendsValue / totalCustMarketValue) * 100 : 0;
      var totalDividendsPercentage = (totalCustMarketValue != 0) ? (totalDividends / totalCustMarketValue) * 100 : 0;

      return new TotalPortfolioPercentageResponse
      {
        PortfolioId = portfolio.Id,
        TotalMarketValue = (decimal)totalMarketValue,
        TotalCustMarketValue = totalCustMarketValue,
        TotalPortfolioProfit = totalWithDividendsMarketValue,
        TotalDifferenceValue = totalDifferenceValue,
        TotalDifferenceWithDividendsPercentage = totalDifferenceDividendsPercentage,
        TotalDifferencePercentage = totalDifferencePercentage,
        TotalDividendsPercentage = totalDividendsPercentage,
        TotalDividends = totalDividends
      };
    }

    private async Task<Dictionary<string, StockData>> GetCurrentPricesAsync(IEnumerable<string> symbols)
    {
      var symbolToStockData = new Dictionary<string, StockData>();

      foreach (var symbol in symbols)
      {
        symbolToStockData[symbol] = await GetCurrentPriceAsync(symbol);
      }

      return symbolToStockData;
    }

    public async Task<PortfolioSentimentImpactResponse> MapSentimentToPortfolioAsync(string userId, string newsText)
    {
      var sentimentResult = await _sentimentAnalysisService.AnalyzeSentimentAsync(newsText);
      var keywords = await _openAIService.GenerateKeywordsAsync(newsText);
      var portfolios = await GetPortfoliosByUserAsync(userId);

      foreach (var portfolio in portfolios)
      {
        foreach (var item in portfolio.Items)
        {
          if (keywords.Contains(item.Symbol))
          {
            item.SentimentImpact = sentimentResult.Score;
            item.AdjustedPrice = item.CurrentPrice * (decimal?)(1 + item.SentimentImpact);
          }
        }
      }

      var totalMarketValueBeforeImpact = portfolios.Sum(p => p.Items.Sum(i => i.CurrentPrice * i.Quantity));
      var totalMarketValueAfterImpact = portfolios.Sum(p => p.Items.Sum(i => (i.AdjustedPrice ?? i.CurrentPrice) * i.Quantity));

      return new PortfolioSentimentImpactResponse
      {
        TotalMarketValueBeforeImpact = (double)totalMarketValueBeforeImpact,
        TotalMarketValueAfterImpact = (double)totalMarketValueAfterImpact,
        PortfolioItems = portfolios.SelectMany(p => p.Items).ToList()
      };
    }

    public async Task<int?> TotalTransactions(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);
      var totalTransactions = portfolios.Select(x => x.Items.Count()).Sum();

      return totalTransactions;
    }

    public async Task<int?> TotalStockEvents(string userId)
    {
      var portfolios = await GetPortfoliosByUserAsync(userId);
      var totalStockEvents = portfolios.Select(x => x.Items.Sum(st => st.StockEvents?.Count() ?? 0)).Sum();

      return totalStockEvents;
    }

    public IEnumerable<dynamic> GetTotalRevenueByMonth(List<Portfolio> portfolios)
    {
      return portfolios
          .SelectMany(p => p.Items)
          .GroupBy(item => new { item.PurchaseDate.Year, item.PurchaseDate.Month })
          .Select(g => new
          {
            Year = g.Key.Year,
            Month = g.Key.Month,
            TotalRevenue = g.Sum(item => item.CurrentMarketValue)
          })
          .ToList();
    }

    public (decimal currentMonthRevenue, decimal previousMonthRevenue) GetMonthRevenues(IEnumerable<dynamic> totalRevenueByMonth)
    {
      var currentYear = DateTime.Now.Year;
      var currentMonth = DateTime.Now.Month;
      var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
      var previousMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

      var currentMonthRevenue = totalRevenueByMonth
          .FirstOrDefault(x => x.Year == currentYear && x.Month == currentMonth)?.TotalRevenue ?? 0;
      var previousMonthRevenue = totalRevenueByMonth
          .FirstOrDefault(x => x.Year == previousMonthYear && x.Month == previousMonth)?.TotalRevenue ?? 0;

      return (currentMonthRevenue, previousMonthRevenue);
    }

    internal async Task<string> AnalyzePortfolioAsync(string[] stockSymbols)
    {
      throw new NotImplementedException();
    }

    // Method to send a portfolio loss alert to the user
    public async Task SendPortfolioLossAlertAsync(string userId, decimal currentValue, decimal lossPercentage)
    {
      // Create the alert message
      string message = $"Alert: Your portfolio has decreased by {lossPercentage:F2}% and is now valued at {currentValue:C}. " +
                       "Please review your investments.";

      // Send the alert message to the specified user via SignalR
      await _hubContext.Clients.User(userId).SendAsync("ReceivePortfolioLossAlert", message);

      // Optionally, log the alert for future reference or auditing purposes
      // Example: Log to database, file, or monitoring service
     await LogPortfolioLossAlertAsync(userId, currentValue, lossPercentage, message);
    }

    //Optional: Method to log the portfolio loss alert(if needed)
    private async Task LogPortfolioLossAlertAsync(string userId, decimal currentValue, decimal lossPercentage, string message)
    {
      // Example logging logic (this could be to a database or an external logging service)
      // Assuming there's a PortfolioAlertLog entity and _context is your database context

      var alertLog = new PortfolioAlertLog
      {
        UserId = userId,
        CurrentValue = currentValue,
        LossPercentage = lossPercentage,
        Message = message,
        Timestamp = DateTime.UtcNow
      };

      await _context.PortfolioAlertLogs.AddAsync(alertLog);
      await _context.SaveChangesAsync();
    }


    internal async Task<List<UserProfile>> GetAllUsersAsync()
    {
      var users=await _context.UserProfiles.ToListAsync();

      return users;
    }

    public async Task<PushSubscription> GetUserPushSubscriptionAsync(string userId)
    {
      // Assume there is a PushSubscriptions table where you store the user's subscription details
      var subscriptionEntity = await _context.PushSubscriptions
          .FirstOrDefaultAsync(sub => sub.UserId == userId);

      if (subscriptionEntity == null)
      {
        return null;
      }

      return new PushSubscription
      {
        Endpoint = subscriptionEntity.Endpoint,
        Keys = new PushSubscriptionKeys
        {
          P256DH = subscriptionEntity.P256DH,
          Auth = subscriptionEntity.Auth
        }
      };
    }

    public async Task<string> GetIndustryBySymbol(string symbol)
    {
     var res=  await YahooService.GetIndustryBySymbolAsync(symbol);
      return res;
    }

 

    // Cálculo Semanal
    public async Task<decimal?> CalculateWeeklyPortfolioPercentageAsync(Portfolio portfolio)
    {
      var historicalDate = DateTime.Now.AddDays(-7);
      decimal historicalValue = 0;

      var tasks = portfolio.Items.Select(async item =>
      {
        var price = await _stockPriceService.GetHistoricalPriceAsync(item.Symbol, historicalDate);
        if (price.Any())
        {
          return item.Quantity * price.First().Close;
        }
        return 0m;
      });

      var results = await Task.WhenAll(tasks);
      historicalValue = results.Sum();

      var currentMarketValue = portfolio.CurrentMarketValue;

      if (historicalValue == 0) return null;

      return (decimal?)CalculatePercentageChange((double)historicalValue, (double)currentMarketValue);
    }

    // Cálculo Mensal
    public async Task<decimal?> CalculateMonthlyPortfolioPercentageAsync(Portfolio portfolio)
    {
      var historicalDate = DateTime.Now.AddMonths(-1);
      decimal historicalValue = 0;

      var tasks = portfolio.Items.Select(async item =>
      {
        var price = await _stockPriceService.GetHistoricalPriceAsync(item.Symbol, historicalDate);
        if (price.Any())
        {
          return item.Quantity * price.First().Close;
        }
        return 0m;
      });

      var results = await Task.WhenAll(tasks);
      historicalValue = results.Sum();

      var currentMarketValue = portfolio.CurrentMarketValue;

      if (historicalValue == 0) return null;

      return (decimal?)CalculatePercentageChange((double)historicalValue, (double)currentMarketValue);
    }

    // Cálculo Anual
    public async Task<decimal?> CalculateYearlyPortfolioPercentageAsync(Portfolio portfolio)
    {
      var historicalDate = DateTime.Now.AddYears(-1);
      decimal historicalValue = 0;

      var tasks = portfolio.Items.Select(async item =>
      {
        var price = await _stockPriceService.GetHistoricalPriceAsync(item.Symbol, historicalDate);
        if (price.Any())
        {
          return item.Quantity * price.First().Close;
        }
        return 0m;
      });

      var results = await Task.WhenAll(tasks);
      historicalValue = results.Sum();

      var currentMarketValue = portfolio.CurrentMarketValue;

      if (historicalValue == 0) return null;

      return (decimal?)CalculatePercentageChange((double)historicalValue, (double)currentMarketValue);
    }

    // Cálculo de perda e gatilho de alerta
    public void CalculateLossAlert(Portfolio portfolio)
    {
      portfolio.LossPercentage = portfolio.TotalGainsLosses < 0
          ? Math.Abs(portfolio.TotalGainsLosses / portfolio.TotalInvestment * 100)
          : 0;

      // Exemplo: disparar alerta se a perda for maior que 10%
      portfolio.IsLossAlertTriggered = portfolio.LossPercentage > 10;
    }

    // Método principal para calcular todas as métricas
    public async Task CalculatePortfolioMetricsAsync(Portfolio portfolio)
    {
      portfolio.WeeklyPercentage = await CalculateWeeklyPortfolioPercentageAsync(portfolio);
      portfolio.MonthlyPercentage = await CalculateMonthlyPortfolioPercentageAsync(portfolio);
      portfolio.YearlyPercentage = await CalculateYearlyPortfolioPercentageAsync(portfolio);

      // Calcular alertas
      CalculateLossAlert(portfolio);
    }

    // Novo: Calcular métricas para um PortfolioItem
    public async Task CalculatePortfolioItemMetricsAsync(PortfolioItem portfolioItem)
    {
      // Exemplo: Calcular variação percentual diária
      if (portfolioItem.PreviousClosePrice.HasValue && portfolioItem.CurrentPrice != 0)
      {
        portfolioItem.Change = portfolioItem.CurrentPrice - portfolioItem.PreviousClosePrice.Value;
        portfolioItem.PercentChange = CalculatePercentageChange(portfolioItem.PreviousClosePrice.Value, portfolioItem.CurrentPrice);
      }

     

      // Outros cálculos baseados em StockEvents, etc., podem ser adicionados aqui
    }






    // DTOs
    public class PortfolioSentimentImpactResponse
    {
      public double TotalMarketValueBeforeImpact { get; set; }
      public double TotalMarketValueAfterImpact { get; set; }
      public List<PortfolioItem> PortfolioItems { get; set; }
    }

    public class PurchaseData
    {
      public DateTime Date { get; set; }
      public int Quantity { get; set; }
      public string OperationType { get; internal set; }
    }
  }
}

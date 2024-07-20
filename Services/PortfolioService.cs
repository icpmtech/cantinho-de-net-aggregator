namespace AspnetCoreMvcFull.Services
{
  using AspnetCoreMvcFull.Models;
  using AspnetCoreMvcFull.Models.Dashboard;
  using AspnetCoreMvcFull.Models.Portfolio;
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

  public class PortfolioService
  {
    private readonly ApplicationDbContext _context;
    private readonly FinnhubService _FinnhubService;
    private readonly ILogger<PortfolioService> _logger;
    private readonly IYahooFinanceService _yahooFinanceService;

    public PortfolioService(ApplicationDbContext context, FinnhubService finnhubService, IYahooFinanceService yahooFinanceService, ILogger<PortfolioService> logger)
    {
      _context = context;
      _FinnhubService = finnhubService;
      _yahooFinanceService = yahooFinanceService;
      _logger = logger;
    }

    // Existing methods

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

    public async Task<IEnumerable<Portfolio>> GetPortfoliosByUserAsync(string userId)
    {
      var portfolios = await _context.Portfolios
                                     .Include(p => p.Items)
                                     .ThenInclude(pi => pi.Dividends)
                                     .Where(p => p.UserId == userId)
                                     .ToListAsync();

      // Update current prices and calculate fields
      foreach (var portfolio in portfolios)
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

    private async Task<StockDataFinHub> GetCurrentPriceAsync(string symbol)
    {
      try
      {
        var stockData = await _FinnhubService.GetRealTimePriceAsync(symbol);
        return stockData;
      }
      catch (Exception ex)
      {
        // Log the exception (optional)
        _logger.LogError(ex, $"Failed to get current price for symbol: {symbol}");
        return new StockDataFinHub
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
        throw new ArgumentException("Invalid portfolio");

      var totalCustMarketValue = portfolio.Items.Sum(item => item.PurchasePrice * item.Quantity);
      var totalMarketValue = portfolio.Items.Sum(item => _yahooFinanceService.GetRealTimePriceAsync(item.Symbol).Result.CurrentPrice * item.Quantity);

      var totalDifferenceValue = totalMarketValue - (double)totalCustMarketValue;
      var totalDifferencePercentage = (totalDifferenceValue / (double)totalCustMarketValue) * 100;

      var itemPercentages = portfolio.Items.Select(item => {
        var currentPrice = _yahooFinanceService.GetRealTimePriceAsync(item.Symbol).Result.CurrentPrice;
        var currentMarketValue = currentPrice * item.Quantity;
        var customMarketValue = item.PurchasePrice * item.Quantity;
        var differenceValue = currentMarketValue - (double)customMarketValue;
        var differencePercentage = ((currentMarketValue - (double)customMarketValue) / (double)customMarketValue) * 100;

        return new PortfolioItemPercentage
        {
          Symbol = item.Symbol,
          CurrentPercentage = (currentMarketValue / totalMarketValue) * 100,
          CustomPercentage = (double)((customMarketValue / totalCustMarketValue) * 100),
          DifferenceValue = differenceValue,
          DifferencePercentage = differencePercentage
        };
      }).ToList();

      return new PortfolioPercentageResponse
      {
        PortfolioId = portfolio.Id,
        TotalMarketValue = (double)totalMarketValue,
        TotalCustMarketValue = (double)totalCustMarketValue,
        TotalDifferenceValue = (double)totalDifferenceValue,
        TotalDifferencePercentage = (double)totalDifferencePercentage,
        ItemPercentages = itemPercentages
      };
    }
  }
}

namespace AspnetCoreMvcFull.Services
{
  using AspnetCoreMvcFull.Models.Dashboard;
  using AspnetCoreMvcFull.Models.Portfolio;
  using ClosedXML.Excel;
  using MarketAnalyticHub.Controllers;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Graph;
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

    public PortfolioService(ApplicationDbContext context, FinnhubService finnhubService, ILogger<PortfolioService> logger)
    {
      _context = context;
      _FinnhubService = finnhubService;
      _logger = logger;
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
          item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
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
          item.CurrentPrice = await GetCurrentPriceAsync(item.Symbol); // Implement this method to fetch the current price
        }
      }

      return portfolio;
    }
    private async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
      try
      {
        return await _FinnhubService.GetRealTimePriceAsync(symbol);

      }
      catch (Exception ex)
      {
        // Log the exception (optional)
         _logger.LogError(ex, $"Failed to get current price for symbol: {symbol}");
        return 0;
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
  }
}

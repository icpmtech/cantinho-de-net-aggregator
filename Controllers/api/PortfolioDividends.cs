using DocumentFormat.OpenXml.Spreadsheet;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Repositories;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class PortfolioDividendsController : ControllerBase
  {
    private readonly PortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IYahooFinanceService _yahooFinanceService;
    private readonly IDataRepository _dataRepository;

    public PortfolioDividendsController(
      PortfolioService portfolioService,
      IDataRepository dataRepository,
      UserManager<ApplicationUser> userManager,
      IYahooFinanceService yahooFinanceService)
    {
      _portfolioService = portfolioService;
      _userManager = userManager;
      _yahooFinanceService = yahooFinanceService;
      _dataRepository = dataRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetDividends()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized();
      }

      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
      if (portfolios == null)
      {
        return NotFound("Portfolios not found.");
      }

      var allDividends = new List<ViewModelPortfolioDividend>();

      foreach (var portfolio in portfolios)
      {
        if (portfolio?.Items == null) continue;

        foreach (var stock in portfolio.Items)
        {
          // Se houver dividendos, adiciona todos
          if (stock?.Dividends != null && stock.Dividends.Any())
          {
            foreach (var dividend in stock.Dividends)
            {
              allDividends.Add(new ViewModelPortfolioDividend
              {
                PurchaseDate=stock.PurchaseDate,
                Quantity= stock.Quantity,
                Id=dividend.Id,
                Symbol = dividend.Symbol,
                PortfolioItemId = dividend.PortfolioItemId,
                Amount = dividend.Amount,
                ExDate = dividend.ExDate,
                PaymentDate = dividend.PaymentDate
              });
            }
          }
          else
          {
            // Caso a ação não tenha dividendos, adiciona entrada com valores nulos/default
            allDividends.Add(new ViewModelPortfolioDividend
            {
              PurchaseDate = stock.PurchaseDate,
              Quantity = stock.Quantity,
              Id = -1,
              Symbol = stock.Symbol,
              PortfolioItemId = stock.Id,
              Amount = 0,
              ExDate = null,
              PaymentDate = null
            });
          }
        }
      }

      return Ok(allDividends);
    }

  }
}

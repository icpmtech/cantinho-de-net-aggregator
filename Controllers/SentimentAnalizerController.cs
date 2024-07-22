using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using MarketAnalyticHub.Services.News;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers
{

  public class SentimentAnalizerController : Controller
  {
   
      private readonly AppNewsService _newsService;
      private readonly PortfolioService _portfolioService;
      private readonly SocialSentimentService _socialSentimentService;

      public SentimentAnalizerController(AppNewsService newsService, PortfolioService portfolioService, SocialSentimentService socialSentimentService)
      {
        _newsService = newsService;
        _portfolioService = portfolioService;
        _socialSentimentService = socialSentimentService;
      }

      public async Task<IActionResult> Index()
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

        foreach (var portfolio in portfolios)
        {
          var portfolioPercentageResponse = _portfolioService.CalculatePortfolioPercentages(portfolio);
          portfolio.PortfolioPercentage += portfolioPercentageResponse.TotalDifferencePercentage;

          // Fetch stock events for portfolio stocks
          foreach (var stock in portfolio.Items)
          {
            stock.StockEvents = await _newsService.GetStockEventsAsync(stock.Symbol);
            stock.SocialSentiment = await _socialSentimentService.GetSocialSentimentAsync(stock.Symbol);
          }
        }

        var stockEvents = portfolios.SelectMany(p => p.Items)
                                    .SelectMany(s => s.StockEvents)
                                    .ToList();

        var positiveEvents = stockEvents.Where(e => e.Sentiment == "Positive").ToList();
        var negativeEvents = stockEvents.Where(e => e.Sentiment == "Negative").ToList();

        var model = new SentimentViewModel
        {
          Portfolios = portfolios,
          PositiveEvents = positiveEvents,
          NegativeEvents = negativeEvents
        };

        return View(model.Portfolios);
      }
    }
  }

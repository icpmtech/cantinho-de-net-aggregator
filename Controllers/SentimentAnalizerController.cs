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
    public async Task<IActionResult> Playground()
    {


      return View();
    }
    public async Task<IActionResult> Forecast()
    {
      

      return View();
    }

    public async Task<IActionResult> ForecastPrice()
    {


      return View();
    }

    public async Task<IActionResult> PredictiveSearch()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);




      return View();
    }

    public async Task<IActionResult> Index()
      {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
      



        return View(portfolios);
      }
    public async Task<IActionResult> Investors()
    {
      return View();
    }
    public async Task<IActionResult> SearchAIPilot()
    {
      return View();
    }
    public async Task<IActionResult> SearchNews()
    {
      return View();
    }
    
    public async Task<IActionResult> SearchSymbols()
    {

      return View();
    }

    public async Task<IActionResult> Calendar()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);




      return View(portfolios);
    }

  }
  }

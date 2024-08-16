using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MarketAnalyticHub.Models;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;
using MarketAnalyticHub.Controllers.api.InvestmentPredictionAPI.Services;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/investment")]
  [ApiController]
  public class InvestmentPredictionAPIController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnallizerController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly SentimentAnalysisService _sentimentAnalysisService;
    private readonly OpenAIService _openAIService;
    private readonly PortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InvestmentService _investmentService;
    public InvestmentPredictionAPIController(UserManager<ApplicationUser> userManager ,ApplicationDbContext context, SentimentAnalysisService sentimentAnalysisService,
      OpenAIService openAIService, HttpClient httpClient, ILogger<AnallizerController> logger, PortfolioService portfolioService)
    {
      _userManager = userManager;
      _context = context;
      _sentimentAnalysisService = sentimentAnalysisService;
      _openAIService = openAIService;
      _httpClient = httpClient;
      _logger = logger;
      _portfolioService= portfolioService;
      _investmentService = new InvestmentService();
    }
   

    [HttpPost("predict")]
    public IActionResult PredictInvestment([FromBody] InvestmentInput input)
    {
      if (input == null)
      {
        return BadRequest("Invalid investment data.");
      }

      var result = _investmentService.PredictInvestment(input);
      return Ok(result);
    }
  }
}

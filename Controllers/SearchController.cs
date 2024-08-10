using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  using AspnetCoreMvcFull.Services;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Dynamic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  
  public class SearchController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly OpenAIService _openAIService;

    public SearchController(ApplicationDbContext context, OpenAIService openAIService)
    {
      _context = context;
      _openAIService = openAIService;
    }




    //[HttpGet]
    //public async Task<IActionResult> Search(string query)
    //{
    //  var model = new SearchViewModel
    //  {
    //    Query = query,
    //    Results = new List<StockEvent>()
    //  };

    //  if (!string.IsNullOrEmpty(query))
    //  {
    //    var openAIResponse = await _openAIService.GetSearchQueryResponse(query);
    //    model.Results = await _context.StockEvents
    //        .Where(item => item.Description.Contains(openAIResponse))
    //         .Where(item => item.Details.Contains(openAIResponse))
    //        .ToListAsync();

    //    if (model.Results.Any())
    //    {
    //      var contentToSummarize = new StringBuilder();
    //      foreach (var item in model.Results)
    //      {
    //        contentToSummarize.AppendLine(item.Description);
    //      }
    //      model.Summary = await _openAIService.SummarizeContent(contentToSummarize.ToString());
    //    }
    //  }

    //  return View(model);
    //}
    [HttpGet]
    public async Task<IActionResult> ChatAiPilot(string query)
    {


      return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index(string query)
    {
     

      return View();
    }
  }

}

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
  
  public class TradingViewController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly OpenAIService _openAIService;

    public TradingViewController(ApplicationDbContext context, OpenAIService openAIService)
    {
      _context = context;
      _openAIService = openAIService;
    }



    public IActionResult Index()
    {
      return View();
    }

    public IActionResult ChartTools()
    {
      return View();
    }

    public IActionResult PatternsIndicators()
    {
      return View();
    }

    public IActionResult SymbolData()
    {
      return View();
    }

    public IActionResult Settings()
    {
      return View();
    }
  }

}

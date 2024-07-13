using AspnetCoreMvcFull.Models.SetupDb;
using AspnetCoreMvcFull.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers.Configurations.Linkedin
{
  public class LinkedinController : Controller
  {
    private readonly NewsService _newsService;
    private readonly ILogger<LinkedinController> _logger;
    private readonly ApplicationDbContext _context;

    public LinkedinController(ApplicationDbContext context, NewsService newsService, ILogger<LinkedinController> logger)
    {
      _newsService = newsService;
      _logger = logger;
      _context = context;
    }
    public IActionResult Index()
    {
      var news = _context.NewsScrapingItem.ToList();

      return View(news);
    }
  }
}

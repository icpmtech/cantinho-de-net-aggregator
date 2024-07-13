using AspnetCoreMvcFull.Models.SetupDb;
using AspnetCoreMvcFull.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers.Configurations.Facebook
{
  public class FacebookController : Controller
  {

    private readonly NewsService _newsService;
    private readonly ILogger<FacebookController> _logger;
    private readonly ApplicationDbContext _context;

    public FacebookController(ApplicationDbContext context, NewsService newsService, ILogger<FacebookController> logger)
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

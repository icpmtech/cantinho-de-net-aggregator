using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using HtmlAgilityPack;

namespace MarketAnalyticHub.Controllers
{
  public class NewsRSSController : Controller
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsRSSController> _logger;

    public NewsRSSController(HttpClient httpClient, ILogger<NewsRSSController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    public async Task<IActionResult> Index()
    {


      return View();
    }
  }
    
}

using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Services.News;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{

  [Route("news")]
  public class NewsController : Controller
  {
    private readonly AppNewsService _newsService;

    public NewsController(AppNewsService newsService)
    {
      _newsService = newsService;
    }
    

    [HttpGet("get")]
    public async Task<IActionResult> Get()
    {
      var newsItems = await _newsService.Get();
    

      return Json(newsItems);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetNewsItem(int id)
    {
      var newsItem = await _newsService.GetNewsByIdAsync(id);
      if (newsItem == null)
      {
        return NotFound();
      }
      return Json(newsItem);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateNewsItem(int id, [FromBody] NewsItem updatedNews)
    {
      if (id != updatedNews.Id)
      {
        return BadRequest();
      }

      var result = await _newsService.UpdateNewsAsync(updatedNews);
      if (result)
      {
        return Json(new { success = true });
      }
      else
      {
        return Json(new { success = false, message = "Update failed" });
      }
    }
  }
}

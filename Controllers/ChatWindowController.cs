using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers
{
  public class ChatWindowController : Controller
  {
    public IActionResult Index()
    {
      return PartialView("_Chat");
    }
  }
}

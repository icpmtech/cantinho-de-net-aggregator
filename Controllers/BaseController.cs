using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Controllers
{
  public abstract class BaseController : ControllerBase
  {
    protected readonly ApplicationDbContext _context;
    protected readonly ILogger<BaseController> _logger;

    protected BaseController(ApplicationDbContext context, ILogger<BaseController> logger)
    {
      _context = context;
      _logger = logger;
    }

    protected async Task<bool> DeductCreditsAsync(string userId, int creditsToDeduct)
    {
      var userCredits = await _context.UserCredits
          .Where(s => s.UserProfile.UserId == userId)
          .FirstOrDefaultAsync();

      if (userCredits == null || userCredits.RemainingCredits < creditsToDeduct)
      {
        return false; // Not enough credits or user not found
      }

      userCredits.UsedCredits += creditsToDeduct;
      await _context.SaveChangesAsync();

      return true;
    }

    protected IActionResult HandleInsufficientCredits()
    {
      return BadRequest(new { success = false, message = "Not enough credits." });
    }
  }


}

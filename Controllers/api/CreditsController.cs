using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class CreditsController : ControllerBase
  {

    private readonly ApplicationDbContext _context;

    public CreditsController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    [Route("current-user")]
    public async Task<IActionResult> GetUserCredits()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      var userCredits = await _context.UserCredits
          .Where(s => s.UserProfile.UserId == userId)
          .FirstOrDefaultAsync();

      if (userCredits == null)
      {
        return NotFound("User credits not found");
      }

      // Calculate the percentage of used credits
      var usedPercentage = (userCredits.TotalCredits > 0)
          ? (userCredits.UsedCredits / (double)userCredits.TotalCredits) * 100
          : 0;

      // Determine if the user has any remaining credits
      var hasCredits = userCredits.RemainingCredits > 0;

      // Return the user credits along with the calculated percentage and credit status
      return Ok(new
      {
        userCredits.TotalCredits,
        userCredits.UsedCredits,
        RemainingCredits = userCredits.RemainingCredits,
        UsedPercentage = usedPercentage,
        HasCredits = hasCredits
      });
    }



    [HttpPost]
    [Route("use/{userId}")]
    public async Task<IActionResult> UseCredits(string userId, [FromBody] int creditsToUse)
    {

      var userCredits = await _context.UserCredits.FindAsync(userId);
      if (userCredits == null)
        return NotFound("User not found");

      if (userCredits.RemainingCredits < creditsToUse)
        return BadRequest("Not enough credits");

      userCredits.UsedCredits += creditsToUse;
      await _context.SaveChangesAsync();

      return Ok(userCredits);
    }


  }
}

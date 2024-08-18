using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {
    private readonly ApplicationDbContext _dbContext;

    public NotificationsController(ApplicationDbContext context)
    {
      _dbContext = context;
    }

    // Endpoint to log a new portfolio alert
    [HttpPost("log-portfolio-alert")]
    public async Task<IActionResult> LogPortfolioAlert([FromBody] PortfolioAlertLog alertLog)
    {
      if (alertLog == null)
      {
        return BadRequest("Invalid alert log data.");
      }

      try
      {
        alertLog.Timestamp = DateTime.UtcNow; // Set the timestamp to the current UTC time
        _dbContext.PortfolioAlertLogs.Add(alertLog);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Success = true, Message = "Portfolio alert logged successfully." });
      }
      catch (Exception ex)
      {
        // Log exception here (e.g., using a logging framework)
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while logging the alert.");
      }
    }

    // Endpoint to get all portfolio alerts for the authenticated user
    [HttpGet("portfolio-alerts")]
    public async Task<IActionResult> GetPortfolioAlerts()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized("User is not authenticated.");
      }

      try
      {
        var alerts = await _dbContext.PortfolioAlertLogs
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new
            {
              a.CurrentValue,
              a.LossPercentage,
              a.Message,
              Timestamp = a.Timestamp.ToString("g") // Adjust the format as needed
            })
            .ToListAsync();

        return Ok(alerts);
      }
      catch (Exception ex)
      {
        // Log exception here
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving alerts.");
      }
    }

    // Endpoint to mark all notifications as read for the authenticated user
    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkNotificationsAsRead()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized("User is not authenticated.");
      }

      try
      {
        var notifications = await _dbContext.PortfolioAlertLogs
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
          notification.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();

        return Ok();
      }
      catch (Exception ex)
      {
        // Log exception here
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while marking notifications as read.");
      }
    }
  }
}

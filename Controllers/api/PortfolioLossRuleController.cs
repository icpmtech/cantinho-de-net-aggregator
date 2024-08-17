using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using MarketAnalyticHub.Models.SetupDb;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class PortfolioLossRuleController : ControllerBase
  {
    private readonly PortfolioLossRuleService _ruleService;
    private readonly ApplicationDbContext _context;

    public PortfolioLossRuleController(ApplicationDbContext context, PortfolioLossRuleService ruleService)
    {
      _ruleService = ruleService;
      _context = context;
    }

    // Retrieve all rules for the authenticated user
    [HttpGet]
    public async Task<IActionResult> GetAllRules()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }

      var userProfile = _context.UserProfiles.FirstOrDefault(s => s.UserId == userId);
      if (userProfile == null)
      {
        return NotFound("User profile not found");
      }

      var rules = await _ruleService.GetRulesByUserAsync(userProfile.Id);
      return Ok(rules);
    }

    // Retrieve a specific rule by Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRuleById(int id)
    {
      var rule = await _ruleService.GetRuleByIdAsync(id);
      if (rule == null)
      {
        return NotFound();
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = _context.UserProfiles.FirstOrDefault(s => s.UserId == userId);
      if (userProfile == null || rule.UserProfileId != userProfile.Id)
      {
        return Unauthorized("You are not authorized to access this rule");
      }

      return Ok(rule);
    }

    // Create a new rule
    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] PortfolioLossRuleViewModel ruleViewModel)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = _context.UserProfiles.FirstOrDefault(s => s.UserId == userId);
      if (userProfile == null)
      {
        return Unauthorized("User profile not found");
      }

      var rule = new PortfolioLossRule
      {
        LossThreshold = ruleViewModel.LossThreshold,
        UserProfileId = userProfile.Id
      };

      await _ruleService.AddRuleAsync(rule);
      return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    // Update an existing rule
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] PortfolioLossRuleViewModel ruleViewModel)
    {
      if (id != ruleViewModel.Id)
      {
        return BadRequest("Rule ID mismatch");
      }

      var existingRule = await _ruleService.GetRuleByIdAsync(id);
      if (existingRule == null)
      {
        return NotFound();
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = _context.UserProfiles.FirstOrDefault(s => s.UserId == userId);
      if (userProfile == null || existingRule.UserProfileId != userProfile.Id)
      {
        return Unauthorized("You are not authorized to update this rule");
      }

      // Update the existing rule with new values
      existingRule.LossThreshold = ruleViewModel.LossThreshold;

      await _ruleService.UpdateRuleAsync(existingRule);
      return NoContent();
    }

    // Delete a rule by its Id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
      var existingRule = await _ruleService.GetRuleByIdAsync(id);
      if (existingRule == null)
      {
        return NotFound();
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = _context.UserProfiles.FirstOrDefault(s => s.UserId == userId);
      if (userProfile == null || existingRule.UserProfileId != userProfile.Id)
      {
        return Unauthorized("You are not authorized to delete this rule");
      }

      await _ruleService.DeleteRuleAsync(id);
      return NoContent();
    }
  }
}

using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class PortfolioLossRuleController : ControllerBase
  {
    private readonly PortfolioLossRuleService _ruleService;

    public PortfolioLossRuleController(PortfolioLossRuleService ruleService)
    {
      _ruleService = ruleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRules()
    {
      var rules = await _ruleService.GetAllRulesAsync();
      return Ok(rules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRuleById(int id)
    {
      var rule = await _ruleService.GetRuleByIdAsync(id);
      if (rule == null)
      {
        return NotFound();
      }
      return Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] PortfolioLossRule rule)
    {
      await _ruleService.AddRuleAsync(rule);
      return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRule(int id, [FromBody] PortfolioLossRule rule)
    {
      if (id != rule.Id)
      {
        return BadRequest();
      }

      var existingRule = await _ruleService.GetRuleByIdAsync(id);
      if (existingRule == null)
      {
        return NotFound();
      }

      await _ruleService.UpdateRuleAsync(rule);
      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRule(int id)
    {
      var existingRule = await _ruleService.GetRuleByIdAsync(id);
      if (existingRule == null)
      {
        return NotFound();
      }

      await _ruleService.DeleteRuleAsync(id);
      return NoContent();
    }
  }

}

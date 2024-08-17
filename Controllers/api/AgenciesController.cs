

namespace MarketAnalyticHub.Controllers.api
{
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  [Route("api/[controller]")]
  [ApiController]
  public class AgenciesController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public AgenciesController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditRatingAgency>>> Get()
    {
      return await _context.CreditRatingAgencies.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CreditRatingAgency>> Get(int id)
    {
      var agency = await _context.CreditRatingAgencies.FindAsync(id);
      if (agency == null)
      {
        return NotFound();
      }
      return agency;
    }

    [HttpPost]
    public async Task<ActionResult<CreditRatingAgency>> Post(CreditRatingAgency agency)
    {
      _context.CreditRatingAgencies.Add(agency);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(Get), new { id = agency.Id }, agency);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, CreditRatingAgency agency)
    {
      if (id != agency.Id)
      {
        return BadRequest();
      }

      _context.Entry(agency).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var agency = await _context.CreditRatingAgencies.FindAsync(id);
      if (agency == null)
      {
        return NotFound();
      }

      _context.CreditRatingAgencies.Remove(agency);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }

}

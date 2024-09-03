using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Controllers.api
{
  [Route("api/[controller]")]
  [ApiController]
  public class CompaniesController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public CompaniesController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
      return await _context.Companies.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> GetCompany(int id)
    {
      var company = await _context.Companies.FindAsync(id);

      if (company == null)
      {
        return NotFound();
      }

      return company;
    }

    [HttpPost]
    public async Task<ActionResult<Company>> PostCompany(Company company)
    {
      _context.Companies.Add(company);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCompany(int id, Company company)
    {
      if (id != company.Id)
      {
        return BadRequest();
      }

      _context.Entry(company).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!CompanyExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
      var company = await _context.Companies.FindAsync(id);
      if (company == null)
      {
        return NotFound();
      }

      _context.Companies.Remove(company);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool CompanyExists(int id)
    {
      return _context.Companies.Any(e => e.Id == id);
    }
  }

}

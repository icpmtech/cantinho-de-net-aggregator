using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/[controller]")]
  public class AssetTransactionUsersController : ControllerBase
  {
    private readonly ApplicationDbContext _db;
    public AssetTransactionUsersController(ApplicationDbContext context)
        => _db = context;

    // GET: api/AssetTransactionUsers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetTransactionUser>>> GetAll()
        => await _db.AssetTransactionUsers.ToListAsync();

    // GET: api/AssetTransactionUsers/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AssetTransactionUser>> Get(int id)
    {
      var tx = await _db.AssetTransactionUsers.FindAsync(id);
      if (tx == null) return NotFound();
      return tx;
    }

    // POST: api/AssetTransactionUsers
    [HttpPost]
    public async Task<ActionResult<AssetTransactionUser>> Create(AssetTransactionUser tx)
    {
      _db.AssetTransactionUsers.Add(tx);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(Get), new { id = tx.Id }, tx);
    }

    // PUT: api/AssetTransactionUsers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AssetTransactionUser tx)
    {
      if (id != tx.Id) return BadRequest();
      _db.Entry(tx).State = EntityState.Modified;
      try { await _db.SaveChangesAsync(); }
      catch (DbUpdateConcurrencyException) when (!_db.AssetTransactionUsers.Any(e => e.Id == id))
      {
        return NotFound();
      }
      return NoContent();
    }

    // DELETE: api/AssetTransactionUsers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      var tx = await _db.AssetTransactionUsers.FindAsync(id);
      if (tx == null) return NotFound();
      _db.AssetTransactionUsers.Remove(tx);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}

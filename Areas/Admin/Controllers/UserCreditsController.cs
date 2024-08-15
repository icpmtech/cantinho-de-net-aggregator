using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Authorization;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserCreditsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserCreditsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/UserCredits
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserCredits.Include(u => u.UserProfile);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/UserCredits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCredit = await _context.UserCredits
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCredit == null)
            {
                return NotFound();
            }

            return View(userCredit);
        }

        // GET: Admin/UserCredits/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.UserProfiles, "Id", "Email");
            return View();
        }

        // POST: Admin/UserCredits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,TotalCredits,UsedCredits")] UserCredit userCredit)
        {

           userCredit.UserProfile=  _context.UserProfiles.Where(s => s.Id == userCredit.UserId).FirstOrDefault();

            if (userCredit.UserProfile is not null)
            {
                _context.Add(userCredit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.UserProfiles, "Id", "Email", userCredit.UserId);
            return View(userCredit);
        }

        // GET: Admin/UserCredits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCredit = await _context.UserCredits.FindAsync(id);
            if (userCredit == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.UserProfiles, "Id", "Email", userCredit.UserId);
            return View(userCredit);
        }

        // POST: Admin/UserCredits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TotalCredits,UsedCredits")] UserCredit userCredit)
        {
            if (id != userCredit.Id)
            {
                return NotFound();
            }

      userCredit.UserProfile = _context.UserProfiles.Where(s => s.Id == userCredit.UserId).FirstOrDefault();

      if (userCredit.UserProfile is not null)
      {
        try
                {
                    _context.Update(userCredit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCreditExists(userCredit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.UserProfiles, "Id", "Email", userCredit.UserId);
            return View(userCredit);
        }

        // GET: Admin/UserCredits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCredit = await _context.UserCredits
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCredit == null)
            {
                return NotFound();
            }

            return View(userCredit);
        }

        // POST: Admin/UserCredits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCredit = await _context.UserCredits.FindAsync(id);
            if (userCredit != null)
            {
                _context.UserCredits.Remove(userCredit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserCreditExists(int id)
        {
            return _context.UserCredits.Any(e => e.Id == id);
        }
    }
}

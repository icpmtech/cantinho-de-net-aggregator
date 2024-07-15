using AspnetCoreMvcFull.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  public class QualitativeEventService : IQualitativeEventService
  {
    private readonly ApplicationDbContext _context;

    public QualitativeEventService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<QualitativeEvent>> GetQualitativeEventsAsync()
    {
      return await _context.QualitativeEvents.ToListAsync();
    }

    public async Task<QualitativeEvent> GetQualitativeEventByIdAsync(int id)
    {
      return await _context.QualitativeEvents.FindAsync(id);
    }

    public async Task AddQualitativeEventAsync(QualitativeEvent qualitativeEvent)
    {
      _context.QualitativeEvents.Add(qualitativeEvent);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateQualitativeEventAsync(QualitativeEvent qualitativeEvent)
    {
      _context.Entry(qualitativeEvent).State = EntityState.Modified;
      await _context.SaveChangesAsync();
    }

    public async Task DeleteQualitativeEventAsync(int id)
    {
      var qualitativeEvent = await _context.QualitativeEvents.FindAsync(id);
      if (qualitativeEvent != null)
      {
        _context.QualitativeEvents.Remove(qualitativeEvent);
        await _context.SaveChangesAsync();
      }
    }
  }

}

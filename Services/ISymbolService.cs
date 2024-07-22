namespace MarketAnalyticHub.Services
{
  public interface ISymbolService
  {
    Task<IEnumerable<string>> GetSymbolsAsync();
  }
}

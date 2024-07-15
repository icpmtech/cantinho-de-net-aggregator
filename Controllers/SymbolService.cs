namespace AspnetCoreMvcFull.Controllers
{
  public class SymbolService : ISymbolService
  {
    public async Task<IEnumerable<string>> GetSymbolsAsync()
    {
      // Return a hardcoded list or fetch from an external API or database
      return await Task.FromResult(new List<string> { "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA" });
    }
  }
}

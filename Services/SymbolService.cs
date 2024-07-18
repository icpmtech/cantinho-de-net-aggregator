using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class SymbolService : ISymbolService
  {
    private readonly ISymbolRepository _symbolRepository;

    public SymbolService(ISymbolRepository symbolRepository)
    {
      _symbolRepository = symbolRepository;
    }

    public async Task<IEnumerable<string>> GetSymbolsAsync()
    {
      var symbols = await _symbolRepository.GetSymbolsAsync();
      return symbols.Select(s => s.Title);
    }
  }

}


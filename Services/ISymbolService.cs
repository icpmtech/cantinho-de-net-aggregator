namespace AspnetCoreMvcFull.Services
{
  public interface ISymbolService
  {
    Task<IEnumerable<string>> GetSymbolsAsync();
  }
}

namespace AspnetCoreMvcFull.Controllers
{
  public interface ISymbolService
  {
    Task<IEnumerable<string>> GetSymbolsAsync();
  }
}

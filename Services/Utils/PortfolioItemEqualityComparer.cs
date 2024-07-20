namespace AspnetCoreMvcFull.Services.Utils
{
  using AspnetCoreMvcFull.Models.Portfolio;
  using System.Collections.Generic;

  public class PortfolioItemSymbolEqualityComparer : IEqualityComparer<PortfolioItem>
  {
    public bool Equals(PortfolioItem x, PortfolioItem y)
    {
      if (x == null || y == null)
        return false;

      return x.Symbol == y.Symbol;
    }

    public int GetHashCode(PortfolioItem obj)
    {
      return obj.Symbol.GetHashCode();
    }
  }

}

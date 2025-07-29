namespace MarketAnalyticHub.Controllers.Api
{
  public class RaciusQuickDto
  {
    private string nif;
    private string nome;
    private string? localidade;

    public RaciusQuickDto(string Nif, string Nome, string? Localidade)
    {
      nif = Nif;
      nome = Nome;
      localidade = Localidade;
    }
  }
}

namespace AspnetCoreMvcFull.Controllers
{
  public partial class QualitativeEventsController
  {
    public class QualitativeEventDto
    {
      public string Symbol { get; set; }
      public string EventDescription { get; set; }
      public DateTime EventDate { get; set; }
      public List<int> NewsIds { get; set; }
    }
  }
  }

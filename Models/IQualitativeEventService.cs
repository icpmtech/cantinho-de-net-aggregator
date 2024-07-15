namespace AspnetCoreMvcFull.Models
{
  public interface IQualitativeEventService
  {
    Task<IEnumerable<QualitativeEvent>> GetQualitativeEventsAsync();
    Task<QualitativeEvent> GetQualitativeEventByIdAsync(int id);
    Task AddQualitativeEventAsync(QualitativeEvent qualitativeEvent);
    Task UpdateQualitativeEventAsync(QualitativeEvent qualitativeEvent);
    Task DeleteQualitativeEventAsync(int id);
  }

}

namespace MarketAnalyticHub.Models
{
  public class ChatMessage
  {
    public int Id { get; set; }
    public string User { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string ChatRoom { get; set; }
    public ICollection<ChatFile> Files { get; set; } // Relationship with ChatFile
  }

  public class ChatFile
  {
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public int ChatMessageId { get; set; }
    public ChatMessage ChatMessage { get; set; }
  }
  public class Chat
  {
    public int Id { get; set; }
    public string User { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string ChatRoom { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } // Relationship with ChatMessages
  }
}

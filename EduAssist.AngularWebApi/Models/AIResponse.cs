namespace EduAssist.AngularWebApi.Models;

public class AIResponse
{
    public int AIResponseId { get; set; }
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UserRequestId { get; set; }

    // Navigation Properties
    public UserRequest UserRequest { get; set; } = null!;
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}

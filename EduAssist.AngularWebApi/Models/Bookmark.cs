namespace EduAssist.AngularWebApi.Models;

public class Bookmark
{
    public int BookmarkId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AIResponseId { get; set; }
    public DateTime BookmarkedOn { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation Properties
    public AIResponse AIResponse { get; set; } = null!;
}

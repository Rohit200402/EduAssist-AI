namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents a bookmarked AI response saved by a student for later revision.
/// Students can add personal notes to their bookmarks.
/// </summary>
public class Bookmark
{
    public int BookmarkId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int AIResponseId { get; set; }

    public DateTime BookmarkedOn { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    // Navigation Properties
    public ApplicationUser User { get; set; } = null!;

    public AIResponse AIResponse { get; set; } = null!;
}

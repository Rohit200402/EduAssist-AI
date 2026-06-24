namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents a bookmarked AI response saved by a student for later revision.
/// Students can add personal notes to their bookmarks.
/// Note: UserId references ApplicationUser in the Auth database (cross-database FK, no navigation).
/// </summary>
public class Bookmark
{
    public int BookmarkId { get; set; }

    /// <summary>
    /// References ApplicationUser.Id from the Authentication database.
    /// No navigation property because it's a cross-database relationship.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public int AIResponseId { get; set; }

    public DateTime BookmarkedOn { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    // Navigation Properties (within same database only)
    public AIResponse AIResponse { get; set; } = null!;
}

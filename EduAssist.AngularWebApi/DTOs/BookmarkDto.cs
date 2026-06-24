using System.ComponentModel.DataAnnotations;

namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// DTO for displaying a bookmarked response.
/// </summary>
public class BookmarkDto
{
    public int BookmarkId { get; set; }

    public int AIResponseId { get; set; }

    public string Query { get; set; } = string.Empty;

    public string Response { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public DateTime BookmarkedOn { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for creating a bookmark.
/// </summary>
public class CreateBookmarkDto
{
    [Required(ErrorMessage = "AI Response ID is required.")]
    public int AIResponseId { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    public string? Notes { get; set; }
}

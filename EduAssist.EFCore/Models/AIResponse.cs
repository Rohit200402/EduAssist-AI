namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents an AI-generated response to a student's question.
/// Multiple responses can exist per UserRequest (regeneration supported).
/// </summary>
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

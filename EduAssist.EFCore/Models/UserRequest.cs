namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents a question asked by a student to the AI Education Assistant.
/// Each request is linked to a user and a category/subject.
/// </summary>
public class UserRequest
{
    public int UserRequestId { get; set; }

    public string Query { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime RequestedOn { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Category Category { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// A student can regenerate responses for the same question (1:Many).
    /// </summary>
    public ICollection<AIResponse> AIResponses { get; set; } = new List<AIResponse>();
}

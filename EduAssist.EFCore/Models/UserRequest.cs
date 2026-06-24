namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents a question asked by a student to the AI Education Assistant.
/// Each request is linked to a user and a category/subject.
/// Note: UserId references ApplicationUser in the Auth database (cross-database FK, no navigation).
/// </summary>
public class UserRequest
{
    public int UserRequestId { get; set; }

    public string Query { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    /// <summary>
    /// References ApplicationUser.Id from the Authentication database.
    /// No navigation property because it's a cross-database relationship.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public DateTime RequestedOn { get; set; } = DateTime.UtcNow;

    // Navigation Properties (within same database only)
    public Category Category { get; set; } = null!;

    /// <summary>
    /// A student can regenerate responses for the same question (1:Many).
    /// </summary>
    public ICollection<AIResponse> AIResponses { get; set; } = new List<AIResponse>();
}

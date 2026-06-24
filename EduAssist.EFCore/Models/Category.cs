namespace EduAssist.EFCore.Models;

/// <summary>
/// Represents an academic subject/category for organizing student questions.
/// Examples: Mathematics, Physics, Chemistry, Biology, etc.
/// </summary>
public class Category
{
    public int CategoryId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<UserRequest> UserRequests { get; set; } = new List<UserRequest>();
}

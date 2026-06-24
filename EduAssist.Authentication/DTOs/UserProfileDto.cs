namespace EduAssist.Authentication.DTOs;

/// <summary>
/// Data Transfer Object for displaying user profile information.
/// Used in profile view, admin user list, and user detail pages.
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Institution { get; set; }

    public string? Grade { get; set; }

    public string? Bio { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime JoinedOn { get; set; }

    public DateTime LastActiveOn { get; set; }

    public bool IsActive { get; set; }

    public string PreferredLanguage { get; set; } = "en";

    public int TotalQueriesAsked { get; set; }

    public string Role { get; set; } = string.Empty;
}

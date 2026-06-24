using System.ComponentModel.DataAnnotations;

namespace EduAssist.Authentication.DTOs;

/// <summary>
/// Data Transfer Object for updating user profile.
/// Used by both the user (editing own profile) and admin (editing any user).
/// </summary>
public class UpdateUserDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Display name cannot exceed 200 characters.")]
    public string? DisplayName { get; set; }

    [StringLength(300, ErrorMessage = "Institution cannot exceed 300 characters.")]
    public string? Institution { get; set; }

    [StringLength(50, ErrorMessage = "Grade cannot exceed 50 characters.")]
    public string? Grade { get; set; }

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    public string? Bio { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(10, ErrorMessage = "Language code cannot exceed 10 characters.")]
    public string? PreferredLanguage { get; set; }
}

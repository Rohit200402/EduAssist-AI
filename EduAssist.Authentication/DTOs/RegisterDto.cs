using System.ComponentModel.DataAnnotations;

namespace EduAssist.Authentication.DTOs;

/// <summary>
/// Data Transfer Object for user registration.
/// Used when a new student or admin registers an account.
/// </summary>
public class RegisterDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Institution cannot exceed 300 characters.")]
    public string? Institution { get; set; }

    [StringLength(50, ErrorMessage = "Grade cannot exceed 50 characters.")]
    public string? Grade { get; set; }
}

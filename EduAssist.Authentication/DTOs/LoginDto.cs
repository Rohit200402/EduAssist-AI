using System.ComponentModel.DataAnnotations;

namespace EduAssist.Authentication.DTOs;

/// <summary>
/// Data Transfer Object for user login.
/// Used when a user authenticates with email and password.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

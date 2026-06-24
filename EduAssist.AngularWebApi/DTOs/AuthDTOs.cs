using System.ComponentModel.DataAnnotations;

namespace EduAssist.AngularWebApi.DTOs;

public class RegisterDto
{
    [Required] [StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required] [StringLength(100)] public string LastName { get; set; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 6)] public string Password { get; set; } = string.Empty;
    [Required] [Compare("Password")] public string ConfirmPassword { get; set; } = string.Empty;
    [StringLength(300)] public string? Institution { get; set; }
    [StringLength(50)] public string? Grade { get; set; }
}

public class LoginDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required] public string CurrentPassword { get; set; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 6)] public string NewPassword { get; set; } = string.Empty;
    [Required] [Compare("NewPassword")] public string ConfirmNewPassword { get; set; } = string.Empty;
}

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

public class CreateUserDto
{
    [Required] [StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required] [StringLength(100)] public string LastName { get; set; } = string.Empty;
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 6)] public string Password { get; set; } = string.Empty;
    [Required] [RegularExpression("^(Admin|User)$")] public string Role { get; set; } = "User";
    [StringLength(300)] public string? Institution { get; set; }
    [StringLength(50)] public string? Grade { get; set; }
}

public class UpdateUserDto
{
    [Required] [StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required] [StringLength(100)] public string LastName { get; set; } = string.Empty;
    [StringLength(200)] public string? DisplayName { get; set; }
    [StringLength(300)] public string? Institution { get; set; }
    [StringLength(50)] public string? Grade { get; set; }
    [StringLength(500)] public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [StringLength(10)] public string? PreferredLanguage { get; set; }
}

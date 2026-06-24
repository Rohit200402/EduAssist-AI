namespace EduAssist.Authentication.DTOs;

/// <summary>
/// Response returned after successful login or registration.
/// Contains the JWT token and user information for the frontend.
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

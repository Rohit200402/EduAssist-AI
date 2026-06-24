using EduAssist.EFCore.Models;

namespace EduAssist.Authentication.Services;

/// <summary>
/// Interface for JWT token generation and management.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for an authenticated user.
    /// Token includes claims: sub (userId), email, name (displayName), role.
    /// </summary>
    /// <param name="user">The authenticated ApplicationUser.</param>
    /// <param name="role">The user's role (Admin or User).</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(ApplicationUser user, string role);
}

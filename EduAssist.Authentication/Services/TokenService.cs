using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduAssist.Authentication.Configuration;
using EduAssist.EFCore.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduAssist.Authentication.Services;

/// <summary>
/// JWT Token Service - Generates signed JWT tokens for authenticated users.
/// Uses HMAC-SHA256 symmetric signing with the secret key from JwtSettings.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Generates a JWT token containing user claims.
    /// 
    /// Claims included:
    /// - sub: User ID (GUID string)
    /// - email: User's email address
    /// - name: User's display name
    /// - role: "Admin" or "User"
    /// - jti: Unique token identifier (for revocation support)
    /// </summary>
    public string GenerateToken(ApplicationUser user, string role)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

namespace EduAssist.Authentication.Configuration;

/// <summary>
/// Strongly-typed configuration class for JWT settings.
/// Maps to the "Jwt" section in appsettings.json.
/// 
/// Example appsettings.json:
/// {
///   "Jwt": {
///     "SecretKey": "YourSuperSecretKeyAtLeast32Characters!",
///     "Issuer": "EduAssist-AI",
///     "Audience": "EduAssist-AI",
///     "ExpirationInMinutes": 60
///   }
/// }
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationInMinutes { get; set; } = 60;
}

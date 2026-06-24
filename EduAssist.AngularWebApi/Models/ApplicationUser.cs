using Microsoft.AspNetCore.Identity;

namespace EduAssist.AngularWebApi.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Institution { get; set; }
    public string? Grade { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveOn { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string PreferredLanguage { get; set; } = "en";
    public int TotalQueriesAsked { get; set; } = 0;
}

using EduAssist.AngularWebApi.Models;

namespace EduAssist.AngularWebApi.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, string role);
}

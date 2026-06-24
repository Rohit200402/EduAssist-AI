using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthService(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService, IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName, LastName = dto.LastName,
            DisplayName = $"{dto.FirstName} {dto.LastName}",
            Institution = dto.Institution, Grade = dto.Grade,
            JoinedOn = DateTime.UtcNow, LastActiveOn = DateTime.UtcNow, IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync("User"))
            await _roleManager.CreateAsync(new IdentityRole("User"));
        await _userManager.AddToRoleAsync(user, "User");

        var token = _tokenService.GenerateToken(user, "User");
        var exp = int.Parse(_config["Jwt:ExpirationInMinutes"] ?? "60");
        return new AuthResponseDto { Token = token, Expiration = DateTime.UtcNow.AddMinutes(exp),
            UserId = user.Id, DisplayName = user.DisplayName, Email = user.Email!, Role = "User" };
    }


    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) throw new UnauthorizedAccessException("Invalid email or password.");
        if (!user.IsActive) throw new UnauthorizedAccessException("Account deactivated.");
        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        user.LastActiveOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = _tokenService.GenerateToken(user, role);
        var exp = int.Parse(_config["Jwt:ExpirationInMinutes"] ?? "60");
        return new AuthResponseDto { Token = token, Expiration = DateTime.UtcNow.AddMinutes(exp),
            UserId = user.Id, DisplayName = user.DisplayName, Email = user.Email!, Role = role };
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        return true;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? "User");
    }

    public async Task<List<UserProfileDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var users = await _userManager.Users.OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var list = new List<UserProfileDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(MapToDto(u, roles.FirstOrDefault() ?? "User"));
        }
        return list;
    }

    public async Task<int> GetUserCountAsync() => await _userManager.Users.CountAsync();


    public async Task<UserProfileDto> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName, LastName = dto.LastName,
            DisplayName = $"{dto.FirstName} {dto.LastName}",
            Institution = dto.Institution, Grade = dto.Grade,
            JoinedOn = DateTime.UtcNow, LastActiveOn = DateTime.UtcNow, IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            await _roleManager.CreateAsync(new IdentityRole(dto.Role));
        await _userManager.AddToRoleAsync(user, dto.Role);
        return MapToDto(user, dto.Role);
    }

    public async Task<UserProfileDto?> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        user.FirstName = dto.FirstName; user.LastName = dto.LastName;
        user.DisplayName = dto.DisplayName ?? $"{dto.FirstName} {dto.LastName}";
        user.Institution = dto.Institution; user.Grade = dto.Grade;
        user.Bio = dto.Bio; user.DateOfBirth = dto.DateOfBirth;
        user.PreferredLanguage = dto.PreferredLanguage ?? "en";
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? "User");
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return (await _userManager.DeleteAsync(user)).Succeeded;
    }

    public async Task<bool> ToggleUserActiveStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    private static UserProfileDto MapToDto(ApplicationUser u, string role) => new()
    {
        Id = u.Id, FirstName = u.FirstName, LastName = u.LastName,
        DisplayName = u.DisplayName, Email = u.Email ?? "",
        Institution = u.Institution, Grade = u.Grade, Bio = u.Bio,
        DateOfBirth = u.DateOfBirth, JoinedOn = u.JoinedOn, LastActiveOn = u.LastActiveOn,
        IsActive = u.IsActive, PreferredLanguage = u.PreferredLanguage,
        TotalQueriesAsked = u.TotalQueriesAsked, Role = role
    };
}


    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) throw new UnauthorizedAccessException("Invalid email or password.");
        if (!user.IsActive) throw new UnauthorizedAccessException("Account deactivated.");
        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        user.LastActiveOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = _tokenService.GenerateToken(user, role);
        var exp = int.Parse(_config["Jwt:ExpirationInMinutes"] ?? "60");
        return new AuthResponseDto { Token = token, Expiration = DateTime.UtcNow.AddMinutes(exp),
            UserId = user.Id, DisplayName = user.DisplayName, Email = user.Email!, Role = role };
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        return true;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? "User");
    }

    public async Task<List<UserProfileDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var users = await _userManager.Users.OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var list = new List<UserProfileDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(MapToDto(u, roles.FirstOrDefault() ?? "User"));
        }
        return list;
    }

    public async Task<int> GetUserCountAsync() => await _userManager.Users.CountAsync();

    public async Task<UserProfileDto> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName, LastName = dto.LastName,
            DisplayName = $"{dto.FirstName} {dto.LastName}",
            Institution = dto.Institution, Grade = dto.Grade,
            JoinedOn = DateTime.UtcNow, LastActiveOn = DateTime.UtcNow, IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            await _roleManager.CreateAsync(new IdentityRole(dto.Role));
        await _userManager.AddToRoleAsync(user, dto.Role);
        return MapToDto(user, dto.Role);
    }

    public async Task<UserProfileDto?> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        user.FirstName = dto.FirstName; user.LastName = dto.LastName;
        user.DisplayName = dto.DisplayName ?? $"{dto.FirstName} {dto.LastName}";
        user.Institution = dto.Institution; user.Grade = dto.Grade;
        user.Bio = dto.Bio; user.DateOfBirth = dto.DateOfBirth;
        user.PreferredLanguage = dto.PreferredLanguage ?? "en";
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? "User");
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        return (await _userManager.DeleteAsync(user)).Succeeded;
    }

    public async Task<bool> ToggleUserActiveStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    private static UserProfileDto MapToDto(ApplicationUser u, string role) => new()
    {
        Id = u.Id, FirstName = u.FirstName, LastName = u.LastName,
        DisplayName = u.DisplayName, Email = u.Email ?? "",
        Institution = u.Institution, Grade = u.Grade, Bio = u.Bio,
        DateOfBirth = u.DateOfBirth, JoinedOn = u.JoinedOn, LastActiveOn = u.LastActiveOn,
        IsActive = u.IsActive, PreferredLanguage = u.PreferredLanguage,
        TotalQueriesAsked = u.TotalQueriesAsked, Role = role
    };
}

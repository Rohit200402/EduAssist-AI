using EduAssist.Authentication.Configuration;
using EduAssist.Authentication.DTOs;
using EduAssist.EFCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EduAssist.Authentication.Services;

/// <summary>
/// Authentication Service - Implements registration, login, password management,
/// and user CRUD operations using ASP.NET Core Identity.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// Registers a new user with the "User" role.
    /// Creates the ApplicationUser, assigns role, and returns JWT token.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Create new ApplicationUser
        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            DisplayName = $"{registerDto.FirstName} {registerDto.LastName}",
            Institution = registerDto.Institution,
            Grade = registerDto.Grade,
            JoinedOn = DateTime.UtcNow,
            LastActiveOn = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        // Ensure "User" role exists and assign it
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }
        await _userManager.AddToRoleAsync(user, "User");

        // Generate JWT token
        var token = _tokenService.GenerateToken(user, "User");

        return new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Role = "User"
        };
    }

    /// <summary>
    /// Authenticates user with email and password.
    /// Validates credentials, checks active status, and returns JWT token.
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if user account is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been deactivated. Please contact an administrator.");
        }

        // Validate password
        var isValidPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Get user's role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        // Update last active timestamp
        user.LastActiveOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate JWT token
        var token = _tokenService.GenerateToken(user, role);

        return new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Role = role
        };
    }

    /// <summary>
    /// Changes the password for the authenticated user.
    /// Requires the current password for verification.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        return true;
    }

    /// <summary>
    /// Retrieves the profile information for a specific user.
    /// </summary>
    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        return MapToProfileDto(user, role);
    }

    /// <summary>
    /// Gets all users with pagination (admin only).
    /// </summary>
    public async Task<List<UserProfileDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userProfiles = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";
            userProfiles.Add(MapToProfileDto(user, role));
        }

        return userProfiles;
    }

    /// <summary>
    /// Gets the total count of users for pagination calculation.
    /// </summary>
    public async Task<int> GetUserCountAsync()
    {
        return await _userManager.Users.CountAsync();
    }

    /// <summary>
    /// Creates a new user with specified role (admin only).
    /// Admin can create both Admin and User accounts.
    /// </summary>
    public async Task<UserProfileDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Create new ApplicationUser
        var user = new ApplicationUser
        {
            UserName = createUserDto.Email,
            Email = createUserDto.Email,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            DisplayName = $"{createUserDto.FirstName} {createUserDto.LastName}",
            Institution = createUserDto.Institution,
            Grade = createUserDto.Grade,
            JoinedOn = DateTime.UtcNow,
            LastActiveOn = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        // Ensure the role exists and assign it
        if (!await _roleManager.RoleExistsAsync(createUserDto.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(createUserDto.Role));
        }
        await _userManager.AddToRoleAsync(user, createUserDto.Role);

        return MapToProfileDto(user, createUserDto.Role);
    }

    /// <summary>
    /// Updates a user's profile information.
    /// Used by both admins (editing any user) and users (editing own profile).
    /// </summary>
    public async Task<UserProfileDto?> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update fields
        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.DisplayName = updateUserDto.DisplayName ?? $"{updateUserDto.FirstName} {updateUserDto.LastName}";
        user.Institution = updateUserDto.Institution;
        user.Grade = updateUserDto.Grade;
        user.Bio = updateUserDto.Bio;
        user.DateOfBirth = updateUserDto.DateOfBirth;
        user.PreferredLanguage = updateUserDto.PreferredLanguage ?? "en";

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User update failed: {errors}");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        return MapToProfileDto(user, role);
    }

    /// <summary>
    /// Permanently deletes a user (admin only).
    /// Returns false if user not found.
    /// </summary>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    /// <summary>
    /// Toggles a user's active/inactive status (admin only).
    /// Inactive users cannot login.
    /// </summary>
    public async Task<bool> ToggleUserActiveStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    /// <summary>
    /// Maps an ApplicationUser entity to a UserProfileDto.
    /// </summary>
    private static UserProfileDto MapToProfileDto(ApplicationUser user, string role)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Institution = user.Institution,
            Grade = user.Grade,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            JoinedOn = user.JoinedOn,
            LastActiveOn = user.LastActiveOn,
            IsActive = user.IsActive,
            PreferredLanguage = user.PreferredLanguage,
            TotalQueriesAsked = user.TotalQueriesAsked,
            Role = role
        };
    }
}

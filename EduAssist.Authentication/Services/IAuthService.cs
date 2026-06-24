using EduAssist.Authentication.DTOs;

namespace EduAssist.Authentication.Services;

/// <summary>
/// Interface for authentication and user management operations.
/// Handles registration, login, password changes, and user CRUD (admin).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user (student) with the "User" role.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Gets the profile of a specific user by ID.
    /// </summary>
    Task<UserProfileDto?> GetUserProfileAsync(string userId);

    /// <summary>
    /// Gets all users (admin only) with pagination support.
    /// </summary>
    Task<List<UserProfileDto>> GetAllUsersAsync(int page, int pageSize);

    /// <summary>
    /// Gets the total count of users (for pagination).
    /// </summary>
    Task<int> GetUserCountAsync();

    /// <summary>
    /// Creates a new user (admin only) with a specified role.
    /// </summary>
    Task<UserProfileDto> CreateUserAsync(CreateUserDto createUserDto);

    /// <summary>
    /// Updates a user's profile (admin or self).
    /// </summary>
    Task<UserProfileDto?> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);

    /// <summary>
    /// Deletes a user (admin only). Hard delete.
    /// </summary>
    Task<bool> DeleteUserAsync(string userId);

    /// <summary>
    /// Toggles a user's active status (admin only).
    /// Inactive users cannot login.
    /// </summary>
    Task<bool> ToggleUserActiveStatusAsync(string userId);
}

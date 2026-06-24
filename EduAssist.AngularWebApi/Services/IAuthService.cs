using EduAssist.AngularWebApi.DTOs;

namespace EduAssist.AngularWebApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
    Task<List<UserProfileDto>> GetAllUsersAsync(int page, int pageSize);
    Task<int> GetUserCountAsync();
    Task<UserProfileDto> CreateUserAsync(CreateUserDto dto);
    Task<UserProfileDto?> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> ToggleUserActiveStatusAsync(string userId);
}

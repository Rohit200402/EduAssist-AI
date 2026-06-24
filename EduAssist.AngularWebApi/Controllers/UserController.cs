using EduAssist.Authentication.DTOs;
using EduAssist.Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAssist.AngularWebApi.Controllers;

/// <summary>
/// User Controller (Admin Only) - Handles user CRUD operations.
/// All endpoints require Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// GET /api/user?page=1&pageSize=5
    /// Gets all users (paginated).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var users = await _authService.GetAllUsersAsync(page, pageSize);
        var totalCount = await _authService.GetUserCountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            data = users,
            currentPage = page,
            pageSize = pageSize,
            totalCount = totalCount,
            totalPages = totalPages,
            hasPrevious = page > 1,
            hasNext = page < totalPages
        });
    }

    /// <summary>
    /// GET /api/user/{id}
    /// Gets a specific user's profile.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _authService.GetUserProfileAsync(id);
        if (user == null) return NotFound(new { error = "User not found." });
        return Ok(user);
    }

    /// <summary>
    /// POST /api/user
    /// Creates a new user (admin can assign role).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var result = await _authService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/user/{id}
    /// Updates a user's profile (admin).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var result = await _authService.UpdateUserAsync(id, updateUserDto);
            if (result == null) return NotFound(new { error = "User not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/user/{id}
    /// Permanently deletes a user.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _authService.DeleteUserAsync(id);
        if (!result) return NotFound(new { error = "User not found." });
        return NoContent();
    }

    /// <summary>
    /// PUT /api/user/{id}/toggle-active
    /// Toggles a user's active/inactive status.
    /// </summary>
    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var result = await _authService.ToggleUserActiveStatusAsync(id);
        if (!result) return NotFound(new { error = "User not found." });
        return Ok(new { message = "User active status toggled successfully." });
    }
}

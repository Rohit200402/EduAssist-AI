using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAssist.AngularWebApi.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var users = await _authService.GetAllUsersAsync(page, pageSize);
        var totalCount = await _authService.GetUserCountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            Data = users,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = page < totalPages
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _authService.GetUserProfileAsync(id);
        if (user == null) return NotFound(new { error = "User not found." });
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var result = await _authService.CreateUserAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var result = await _authService.UpdateUserAsync(id, dto);
            if (result == null) return NotFound(new { error = "User not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var result = await _authService.DeleteUserAsync(id);
            if (!result) return NotFound(new { error = "User not found." });
            return Ok(new { message = "User deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(string id)
    {
        try
        {
            var result = await _authService.ToggleUserActiveStatusAsync(id);
            if (!result) return NotFound(new { error = "User not found." });
            return Ok(new { message = "User active status toggled successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

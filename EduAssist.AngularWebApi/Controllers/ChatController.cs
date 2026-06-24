using System.Security.Claims;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAssist.AngularWebApi.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] ChatRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        try
        {
            var result = await _chatService.AskQuestionAsync(dto, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(503, new { error = "AI service is currently unavailable. Please try again later." });
        }
    }

    [HttpPost("regenerate/{userRequestId}")]
    public async Task<IActionResult> RegenerateResponse(int userRequestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        try
        {
            var result = await _chatService.RegenerateResponseAsync(userRequestId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(503, new { error = "AI service is currently unavailable. Please try again later." });
        }
    }
}

using System.Security.Claims;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAssist.AngularWebApi.Controllers;

/// <summary>
/// Chat Controller - Core AI functionality.
/// Handles asking questions and regenerating responses.
/// Rate limited: 20 requests/day per student (middleware handles this).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// POST /api/chat/ask
    /// Submits a question to the AI and receives an explanation.
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] ChatRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _chatService.AskQuestionAsync(request, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/chat/regenerate/{userRequestId}
    /// Regenerates a new AI response for an existing question.
    /// </summary>
    [HttpPost("regenerate/{userRequestId}")]
    public async Task<IActionResult> RegenerateResponse(int userRequestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _chatService.RegenerateResponseAsync(userRequestId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }
}

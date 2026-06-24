using System.Security.Claims;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Services;
using EduAssist.EFCore.Context;
using EduAssist.EFCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

/// <summary>
/// History Controller - Q&A history, search, and bookmarks.
/// Students see their own history. Admins see all.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly AppDbContext _appDbContext;

    public HistoryController(ISearchService searchService, AppDbContext appDbContext)
    {
        _searchService = searchService;
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// GET /api/history?keyword=...&categoryId=...&page=1&pageSize=5
    /// Gets the current user's Q&A history (paginated, filterable).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyHistory([FromQuery] SearchRequestDto searchParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var result = await _searchService.GetUserHistoryAsync(userId, searchParams);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/history/all?keyword=...&userId=...&page=1 (Admin only)
    /// Gets all users' Q&A history.
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllHistory([FromQuery] SearchRequestDto searchParams)
    {
        var result = await _searchService.GetAllHistoryAsync(searchParams);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/history/{userRequestId}
    /// Gets the full detail of a specific Q&A session.
    /// </summary>
    [HttpGet("{userRequestId}")]
    public async Task<IActionResult> GetDetail(int userRequestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var isAdmin = User.IsInRole("Admin");
        var result = await _searchService.GetHistoryDetailAsync(userRequestId, userId, isAdmin);

        if (result == null)
            return NotFound(new { error = "Q&A session not found." });

        return Ok(result);
    }

    /// <summary>
    /// POST /api/history/bookmark
    /// Bookmarks an AI response for later revision.
    /// </summary>
    [HttpPost("bookmark")]
    public async Task<IActionResult> CreateBookmark([FromBody] CreateBookmarkDto bookmarkDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // Verify the AI response exists
        var aiResponse = await _appDbContext.AIResponses
            .Include(ar => ar.UserRequest)
            .FirstOrDefaultAsync(ar => ar.AIResponseId == bookmarkDto.AIResponseId);

        if (aiResponse == null)
            return NotFound(new { error = "AI response not found." });

        // Check if already bookmarked
        var existingBookmark = await _appDbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.AIResponseId == bookmarkDto.AIResponseId);

        if (existingBookmark != null)
            return BadRequest(new { error = "You have already bookmarked this response." });

        var bookmark = new Bookmark
        {
            UserId = userId,
            AIResponseId = bookmarkDto.AIResponseId,
            BookmarkedOn = DateTime.UtcNow,
            Notes = bookmarkDto.Notes
        };

        _appDbContext.Bookmarks.Add(bookmark);
        await _appDbContext.SaveChangesAsync();

        return StatusCode(201, new { message = "Bookmark created successfully.", bookmarkId = bookmark.BookmarkId });
    }

    /// <summary>
    /// GET /api/history/bookmarks?page=1&pageSize=5
    /// Gets the current user's bookmarked responses.
    /// </summary>
    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var query = _appDbContext.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.AIResponse)
                .ThenInclude(ar => ar.UserRequest)
                    .ThenInclude(ur => ur.Category)
            .OrderByDescending(b => b.BookmarkedOn);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var bookmarks = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookmarkDto
            {
                BookmarkId = b.BookmarkId,
                AIResponseId = b.AIResponseId,
                Query = b.AIResponse.UserRequest.Query,
                Response = b.AIResponse.Response,
                SubjectName = b.AIResponse.UserRequest.Category.SubjectName,
                BookmarkedOn = b.BookmarkedOn,
                Notes = b.Notes
            })
            .ToListAsync();

        return Ok(new PaginatedResponseDto<BookmarkDto>
        {
            Data = bookmarks,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = page < totalPages
        });
    }

    /// <summary>
    /// DELETE /api/history/bookmark/{id}
    /// Removes a bookmark.
    /// </summary>
    [HttpDelete("bookmark/{id}")]
    public async Task<IActionResult> DeleteBookmark(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var bookmark = await _appDbContext.Bookmarks
            .FirstOrDefaultAsync(b => b.BookmarkId == id && b.UserId == userId);

        if (bookmark == null)
            return NotFound(new { error = "Bookmark not found." });

        _appDbContext.Bookmarks.Remove(bookmark);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }
}

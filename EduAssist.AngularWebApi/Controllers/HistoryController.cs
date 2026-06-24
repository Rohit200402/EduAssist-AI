using System.Security.Claims;
using EduAssist.AngularWebApi.Context;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Models;
using EduAssist.AngularWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

[ApiController]
[Route("api/history")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly AppDbContext _context;

    public HistoryController(ISearchService searchService, AppDbContext context)
    {
        _searchService = searchService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyHistory([FromQuery] SearchRequestDto searchParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var result = await _searchService.GetUserHistoryAsync(userId, searchParams);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllHistory([FromQuery] SearchRequestDto searchParams)
    {
        var result = await _searchService.GetAllHistoryAsync(searchParams);
        return Ok(result);
    }

    [HttpGet("{userRequestId}")]
    public async Task<IActionResult> GetHistoryDetail(int userRequestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var isAdmin = User.IsInRole("Admin");
        var result = await _searchService.GetHistoryDetailAsync(userRequestId, userId, isAdmin);
        if (result == null) return NotFound(new { error = "History record not found." });

        return Ok(result);
    }

    [HttpPost("bookmark")]
    public async Task<IActionResult> CreateBookmark([FromBody] CreateBookmarkDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var aiResponse = await _context.AIResponses
            .Include(a => a.UserRequest)
                .ThenInclude(ur => ur.Category)
            .FirstOrDefaultAsync(a => a.AIResponseId == dto.AIResponseId);

        if (aiResponse == null) return NotFound(new { error = "AI response not found." });

        var duplicate = await _context.Bookmarks
            .AnyAsync(b => b.UserId == userId && b.AIResponseId == dto.AIResponseId);

        if (duplicate) return BadRequest(new { error = "This response is already bookmarked." });

        var bookmark = new Bookmark
        {
            UserId = userId,
            AIResponseId = dto.AIResponseId,
            Notes = dto.Notes,
            BookmarkedOn = DateTime.UtcNow
        };

        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync();

        return Ok(new BookmarkDto
        {
            BookmarkId = bookmark.BookmarkId,
            AIResponseId = bookmark.AIResponseId,
            Query = aiResponse.UserRequest.Query,
            Response = aiResponse.Response,
            SubjectName = aiResponse.UserRequest.Category.SubjectName,
            BookmarkedOn = bookmark.BookmarkedOn,
            Notes = bookmark.Notes
        });
    }

    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var query = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.AIResponse)
                .ThenInclude(a => a.UserRequest)
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

        return Ok(new
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

    [HttpDelete("bookmark/{id}")]
    public async Task<IActionResult> DeleteBookmark(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.BookmarkId == id && b.UserId == userId);

        if (bookmark == null) return NotFound(new { error = "Bookmark not found." });

        _context.Bookmarks.Remove(bookmark);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Bookmark deleted successfully." });
    }
}

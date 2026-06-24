using System.Security.Claims;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.EFCore.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

/// <summary>
/// Progress Controller - Student learning stats.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public ProgressController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }


    /// <summary>
    /// GET /api/progress/stats
    /// Gets overall progress statistics for the current user.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var userRequests = _appDbContext.UserRequests
            .Where(ur => ur.UserId == userId);

        var total = await userRequests.CountAsync();
        var thisWeek = await userRequests
            .CountAsync(ur => ur.RequestedOn >= startOfWeek);
        var thisMonth = await userRequests
            .CountAsync(ur => ur.RequestedOn >= startOfMonth);

        // Category breakdown
        var breakdown = await userRequests
            .GroupBy(ur => ur.Category.SubjectName)
            .Select(g => new CategoryBreakdownDto
            {
                SubjectName = g.Key,
                Count = g.Count(),
                Percentage = total > 0
                    ? Math.Round((double)g.Count() / total * 100, 1)
                    : 0
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        var favoriteSubject = breakdown.FirstOrDefault()?.SubjectName;

        // Calculate streak
        var streak = await CalculateStreakAsync(userId);

        return Ok(new ProgressDto
        {
            TotalQuestionsAsked = total,
            QuestionsThisWeek = thisWeek,
            QuestionsThisMonth = thisMonth,
            CurrentStreak = streak,
            FavoriteSubject = favoriteSubject,
            CategoryBreakdown = breakdown
        });
    }


    /// <summary>
    /// Calculates the current consecutive-day streak for a user.
    /// </summary>
    private async Task<int> CalculateStreakAsync(string userId)
    {
        var dates = await _appDbContext.UserRequests
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RequestedOn.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        if (dates.Count == 0) return 0;

        int streak = 0;
        var today = DateTime.UtcNow.Date;

        // Check if user was active today or yesterday
        if (dates[0] != today && dates[0] != today.AddDays(-1))
            return 0;

        var expectedDate = dates[0];
        foreach (var date in dates)
        {
            if (date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }
}

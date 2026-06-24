using System.Security.Claims;
using EduAssist.AngularWebApi.Context;
using EduAssist.AngularWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized(new { error = "User not found." });

        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var userRequests = _context.UserRequests
            .Where(ur => ur.UserId == userId);

        var totalQuestions = await userRequests.CountAsync();
        var questionsThisWeek = await userRequests.CountAsync(ur => ur.RequestedOn >= startOfWeek);
        var questionsThisMonth = await userRequests.CountAsync(ur => ur.RequestedOn >= startOfMonth);

        var categoryBreakdown = await userRequests
            .GroupBy(ur => ur.Category.SubjectName)
            .Select(g => new CategoryBreakdownDto
            {
                SubjectName = g.Key,
                Count = g.Count(),
                Percentage = totalQuestions > 0 ? Math.Round(g.Count() * 100.0 / totalQuestions, 1) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        var favoriteSubject = categoryBreakdown.FirstOrDefault()?.SubjectName;

        // Streak calculation
        var distinctDates = await userRequests
            .Select(ur => ur.RequestedOn.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        var streak = 0;
        if (distinctDates.Any())
        {
            var today = now.Date;
            var checkDate = today;

            // Start from today or yesterday
            if (distinctDates[0] == today)
            {
                checkDate = today;
            }
            else if (distinctDates[0] == today.AddDays(-1))
            {
                checkDate = today.AddDays(-1);
            }
            else
            {
                checkDate = DateTime.MinValue; // No streak
            }

            if (checkDate != DateTime.MinValue)
            {
                foreach (var date in distinctDates)
                {
                    if (date == checkDate)
                    {
                        streak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                    else if (date < checkDate)
                    {
                        break;
                    }
                }
            }
        }

        var progress = new ProgressDto
        {
            TotalQuestionsAsked = totalQuestions,
            QuestionsThisWeek = questionsThisWeek,
            QuestionsThisMonth = questionsThisMonth,
            CurrentStreak = streak,
            FavoriteSubject = favoriteSubject,
            CategoryBreakdown = categoryBreakdown
        };

        return Ok(progress);
    }
}

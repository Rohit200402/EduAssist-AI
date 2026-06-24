using System.Collections.Concurrent;
using System.Security.Claims;

namespace EduAssist.AngularWebApi.Middleware;

/// <summary>
/// Rate Limiting Middleware - Limits AI chat requests per user per day.
/// Only applies to the /api/chat endpoints.
/// Admins are exempt from rate limiting.
/// 
/// Uses in-memory tracking (resets on app restart).
/// For production, use Redis or database-backed tracking.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private static readonly ConcurrentDictionary<string, UserRequestTracker> _trackers = new();

    public RateLimitingMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to chat endpoints
        if (!context.Request.Path.StartsWithSegments("/api/chat"))
        {
            await _next(context);
            return;
        }

        // Only apply to POST requests (asking questions / regenerating)
        if (context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }

        // Skip rate limiting for unauthenticated requests (they'll fail at auth anyway)
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await _next(context);
            return;
        }

        // Admins are exempt from rate limiting
        var isAdmin = context.User.IsInRole("Admin");
        if (isAdmin)
        {
            await _next(context);
            return;
        }

        // Check rate limit
        var maxRequests = _configuration.GetValue<int>("RateLimit:MaxRequestsPerDay", 20);
        var tracker = _trackers.GetOrAdd(userId, _ => new UserRequestTracker());

        // Reset counter if it's a new day
        if (tracker.LastResetDate.Date != DateTime.UtcNow.Date)
        {
            tracker.Reset();
        }

        if (tracker.RequestCount >= maxRequests)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                $"{{\"error\": \"Rate limit exceeded. You can ask a maximum of {maxRequests} questions per day. Please try again tomorrow.\"}}");
            return;
        }

        // Increment counter and proceed
        tracker.RequestCount++;
        await _next(context);
    }

    /// <summary>
    /// Tracks request count per user per day.
    /// </summary>
    private class UserRequestTracker
    {
        public int RequestCount { get; set; }
        public DateTime LastResetDate { get; set; } = DateTime.UtcNow;

        public void Reset()
        {
            RequestCount = 0;
            LastResetDate = DateTime.UtcNow;
        }
    }
}

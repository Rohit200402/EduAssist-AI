using System.Collections.Concurrent;
using System.Security.Claims;

namespace EduAssist.AngularWebApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private static readonly ConcurrentDictionary<string, (int count, DateTime date)> _tracker = new();

    public RateLimitingMiddleware(RequestDelegate next, IConfiguration config)
    { _next = next; _config = config; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!ctx.Request.Path.StartsWithSegments("/api/chat") || ctx.Request.Method != "POST")
        { await _next(ctx); return; }

        var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || ctx.User.IsInRole("Admin"))
        { await _next(ctx); return; }

        var max = _config.GetValue<int>("RateLimit:MaxRequestsPerDay", 20);
        var entry = _tracker.GetOrAdd(userId, _ => (0, DateTime.UtcNow));
        if (entry.date.Date != DateTime.UtcNow.Date) entry = (0, DateTime.UtcNow);
        if (entry.count >= max)
        {
            ctx.Response.StatusCode = 429;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync($"{{\"error\":\"Rate limit exceeded. Max {max} questions/day.\"}}");
            return;
        }
        _tracker[userId] = (entry.count + 1, entry.date);
        await _next(ctx);
    }
}

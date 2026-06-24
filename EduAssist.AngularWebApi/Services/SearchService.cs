using EduAssist.AngularWebApi.Context;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Services;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;
    public SearchService(AppDbContext db) { _db = db; }

    public async Task<PaginatedResponseDto<HistoryDto>> GetUserHistoryAsync(string userId, SearchRequestDto p)
    {
        var q = _db.UserRequests.Include(ur => ur.Category).Include(ur => ur.AIResponses)
            .Where(ur => ur.UserId == userId).AsQueryable();
        return await Paginate(ApplyFilters(q, p), p.Page, p.PageSize);
    }

    public async Task<PaginatedResponseDto<HistoryDto>> GetAllHistoryAsync(SearchRequestDto p)
    {
        var q = _db.UserRequests.Include(ur => ur.Category).Include(ur => ur.AIResponses).AsQueryable();
        if (!string.IsNullOrEmpty(p.UserId)) q = q.Where(ur => ur.UserId == p.UserId);
        return await Paginate(ApplyFilters(q, p), p.Page, p.PageSize);
    }

    public async Task<HistoryDetailDto?> GetHistoryDetailAsync(int id, string userId, bool isAdmin)
    {
        var q = _db.UserRequests.Include(ur => ur.Category).Include(ur => ur.AIResponses).AsQueryable();
        if (!isAdmin) q = q.Where(ur => ur.UserId == userId);
        var ur = await q.FirstOrDefaultAsync(x => x.UserRequestId == id);
        if (ur == null) return null;
        return new HistoryDetailDto
        {
            UserRequestId = ur.UserRequestId, Query = ur.Query,
            SubjectName = ur.Category.SubjectName, RequestedOn = ur.RequestedOn, UserId = ur.UserId,
            Responses = ur.AIResponses.OrderByDescending(a => a.CreatedAt)
                .Select(a => new AIResponseItemDto { AIResponseId = a.AIResponseId, Response = a.Response, CreatedAt = a.CreatedAt }).ToList()
        };
    }

    private static IQueryable<UserRequest> ApplyFilters(IQueryable<UserRequest> q, SearchRequestDto p)
    {
        if (!string.IsNullOrEmpty(p.Keyword)) q = q.Where(ur => ur.Query.Contains(p.Keyword));
        if (p.CategoryId.HasValue) q = q.Where(ur => ur.CategoryId == p.CategoryId.Value);
        if (p.FromDate.HasValue) q = q.Where(ur => ur.RequestedOn >= p.FromDate.Value);
        if (p.ToDate.HasValue) q = q.Where(ur => ur.RequestedOn <= p.ToDate.Value);
        return q.OrderByDescending(ur => ur.RequestedOn);
    }


    private static async Task<PaginatedResponseDto<HistoryDto>> Paginate(IQueryable<UserRequest> q, int page, int pageSize)
    {
        var total = await q.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(ur => new HistoryDto
            {
                UserRequestId = ur.UserRequestId, Query = ur.Query,
                SubjectName = ur.Category.SubjectName, RequestedOn = ur.RequestedOn,
                ResponseCount = ur.AIResponses.Count
            }).ToListAsync();
        return new PaginatedResponseDto<HistoryDto>
        {
            Data = items, CurrentPage = page, PageSize = pageSize,
            TotalCount = total, TotalPages = totalPages,
            HasPrevious = page > 1, HasNext = page < totalPages
        };
    }
}

using EduAssist.AngularWebApi.DTOs;
using EduAssist.EFCore.Context;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// Search Service - Handles history retrieval with filtering and pagination.
/// Supports keyword search, category filter, date range, and user isolation.
/// </summary>
public class SearchService : ISearchService
{
    private readonly AppDbContext _appDbContext;

    public SearchService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// Gets paginated Q&A history for a specific user.
    /// </summary>
    public async Task<PaginatedResponseDto<HistoryDto>> GetUserHistoryAsync(
        string userId, SearchRequestDto searchParams)
    {
        var query = _appDbContext.UserRequests
            .Include(ur => ur.Category)
            .Include(ur => ur.AIResponses)
            .Where(ur => ur.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, searchParams);
        return await GetPaginatedResultAsync(query, searchParams.Page, searchParams.PageSize);
    }

    /// <summary>
    /// Gets paginated Q&A history for all users (admin).
    /// </summary>
    public async Task<PaginatedResponseDto<HistoryDto>> GetAllHistoryAsync(
        SearchRequestDto searchParams)
    {
        var query = _appDbContext.UserRequests
            .Include(ur => ur.Category)
            .Include(ur => ur.AIResponses)
            .AsQueryable();

        // Admin can filter by specific user
        if (!string.IsNullOrEmpty(searchParams.UserId))
        {
            query = query.Where(ur => ur.UserId == searchParams.UserId);
        }

        query = ApplyFilters(query, searchParams);
        return await GetPaginatedResultAsync(query, searchParams.Page, searchParams.PageSize);
    }

    /// <summary>
    /// Gets full detail of a Q&A session including all AI responses.
    /// </summary>
    public async Task<HistoryDetailDto?> GetHistoryDetailAsync(
        int userRequestId, string userId, bool isAdmin)
    {
        var query = _appDbContext.UserRequests
            .Include(ur => ur.Category)
            .Include(ur => ur.AIResponses)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(ur => ur.UserId == userId);
        }

        var userRequest = await query
            .FirstOrDefaultAsync(ur => ur.UserRequestId == userRequestId);

        if (userRequest == null) return null;

        return new HistoryDetailDto
        {
            UserRequestId = userRequest.UserRequestId,
            Query = userRequest.Query,
            SubjectName = userRequest.Category.SubjectName,
            RequestedOn = userRequest.RequestedOn,
            UserId = userRequest.UserId,
            Responses = userRequest.AIResponses
                .OrderByDescending(ar => ar.CreatedAt)
                .Select(ar => new AIResponseDto
                {
                    AIResponseId = ar.AIResponseId,
                    Response = ar.Response,
                    CreatedAt = ar.CreatedAt
                }).ToList()
        };
    }

    /// <summary>
    /// Applies keyword, category, and date filters to the query.
    /// </summary>
    private static IQueryable<EduAssist.EFCore.Models.UserRequest> ApplyFilters(
        IQueryable<EduAssist.EFCore.Models.UserRequest> query, SearchRequestDto searchParams)
    {
        if (!string.IsNullOrEmpty(searchParams.Keyword))
        {
            query = query.Where(ur => ur.Query.Contains(searchParams.Keyword));
        }

        if (searchParams.CategoryId.HasValue)
        {
            query = query.Where(ur => ur.CategoryId == searchParams.CategoryId.Value);
        }

        if (searchParams.FromDate.HasValue)
        {
            query = query.Where(ur => ur.RequestedOn >= searchParams.FromDate.Value);
        }

        if (searchParams.ToDate.HasValue)
        {
            query = query.Where(ur => ur.RequestedOn <= searchParams.ToDate.Value);
        }

        return query.OrderByDescending(ur => ur.RequestedOn);
    }

    /// <summary>
    /// Applies pagination and maps results to DTOs.
    /// </summary>
    private static async Task<PaginatedResponseDto<HistoryDto>> GetPaginatedResultAsync(
        IQueryable<EduAssist.EFCore.Models.UserRequest> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ur => new HistoryDto
            {
                UserRequestId = ur.UserRequestId,
                Query = ur.Query,
                SubjectName = ur.Category.SubjectName,
                RequestedOn = ur.RequestedOn,
                ResponseCount = ur.AIResponses.Count
            })
            .ToListAsync();

        return new PaginatedResponseDto<HistoryDto>
        {
            Data = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = page < totalPages
        };
    }
}

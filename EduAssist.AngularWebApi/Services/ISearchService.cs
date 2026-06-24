using EduAssist.AngularWebApi.DTOs;

namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// Interface for search and history retrieval operations.
/// Supports keyword search, category filter, date range, and pagination.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Gets paginated Q&A history for a specific user (student view).
    /// </summary>
    Task<PaginatedResponseDto<HistoryDto>> GetUserHistoryAsync(string userId, SearchRequestDto searchParams);

    /// <summary>
    /// Gets paginated Q&A history for all users (admin view).
    /// </summary>
    Task<PaginatedResponseDto<HistoryDto>> GetAllHistoryAsync(SearchRequestDto searchParams);

    /// <summary>
    /// Gets the full detail of a specific Q&A session (all AI responses).
    /// </summary>
    Task<HistoryDetailDto?> GetHistoryDetailAsync(int userRequestId, string userId, bool isAdmin);
}

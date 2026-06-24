using EduAssist.AngularWebApi.DTOs;

namespace EduAssist.AngularWebApi.Services;

public interface ISearchService
{
    Task<PaginatedResponseDto<HistoryDto>> GetUserHistoryAsync(string userId, SearchRequestDto searchParams);
    Task<PaginatedResponseDto<HistoryDto>> GetAllHistoryAsync(SearchRequestDto searchParams);
    Task<HistoryDetailDto?> GetHistoryDetailAsync(int userRequestId, string userId, bool isAdmin);
}

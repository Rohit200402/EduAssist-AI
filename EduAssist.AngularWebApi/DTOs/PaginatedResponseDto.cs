namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// Generic paginated response wrapper.
/// Used for all list endpoints with 5 records per page.
/// </summary>
public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = new List<T>();

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPrevious { get; set; }

    public bool HasNext { get; set; }
}

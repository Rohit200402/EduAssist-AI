namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// Request DTO for searching/filtering Q&A history.
/// All fields are optional filters. Pagination defaults to page 1, size 5.
/// </summary>
public class SearchRequestDto
{
    public string? Keyword { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Admin-only: Filter by specific user ID.
    /// </summary>
    public string? UserId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 5;
}

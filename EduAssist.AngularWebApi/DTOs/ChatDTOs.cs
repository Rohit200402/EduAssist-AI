using System.ComponentModel.DataAnnotations;

namespace EduAssist.AngularWebApi.DTOs;

public class ChatRequestDto
{
    [Required] [StringLength(2000, MinimumLength = 5)] public string Query { get; set; } = string.Empty;
    [Required] [Range(1, int.MaxValue)] public int CategoryId { get; set; }
}

public class ChatResponseDto
{
    public int UserRequestId { get; set; }
    public int AIResponseId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SearchRequestDto
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class HistoryDto
{
    public int UserRequestId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime RequestedOn { get; set; }
    public int ResponseCount { get; set; }
}

public class HistoryDetailDto
{
    public int UserRequestId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime RequestedOn { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<AIResponseItemDto> Responses { get; set; } = new();
}

public class AIResponseItemDto
{
    public int AIResponseId { get; set; }
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    [Required] [StringLength(150, MinimumLength = 2)] public string SubjectName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
}

public class ProgressDto
{
    public int TotalQuestionsAsked { get; set; }
    public int QuestionsThisWeek { get; set; }
    public int QuestionsThisMonth { get; set; }
    public int CurrentStreak { get; set; }
    public string? FavoriteSubject { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public string SubjectName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class BookmarkDto
{
    public int BookmarkId { get; set; }
    public int AIResponseId { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime BookmarkedOn { get; set; }
    public string? Notes { get; set; }
}

public class CreateBookmarkDto
{
    [Required] public int AIResponseId { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

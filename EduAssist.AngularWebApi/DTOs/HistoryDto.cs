namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// DTO for history list items (summary view).
/// </summary>
public class HistoryDto
{
    public int UserRequestId { get; set; }

    public string Query { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public DateTime RequestedOn { get; set; }

    public int ResponseCount { get; set; }
}

/// <summary>
/// DTO for history detail view (full Q&A with all responses).
/// </summary>
public class HistoryDetailDto
{
    public int UserRequestId { get; set; }

    public string Query { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public DateTime RequestedOn { get; set; }

    public string UserId { get; set; } = string.Empty;

    public List<AIResponseDto> Responses { get; set; } = new List<AIResponseDto>();
}

/// <summary>
/// DTO for individual AI response within a history detail.
/// </summary>
public class AIResponseDto
{
    public int AIResponseId { get; set; }

    public string Response { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

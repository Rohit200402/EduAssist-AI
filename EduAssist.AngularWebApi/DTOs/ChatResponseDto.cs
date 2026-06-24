namespace EduAssist.AngularWebApi.DTOs;

/// <summary>
/// Response DTO returned after AI generates an answer.
/// </summary>
public class ChatResponseDto
{
    public int UserRequestId { get; set; }

    public int AIResponseId { get; set; }

    public string Query { get; set; } = string.Empty;

    public string Response { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

using EduAssist.AngularWebApi.DTOs;

namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// Interface for the Chat business logic service.
/// Orchestrates the flow: save question → call AI → save response → return.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Processes a student's question: saves it, calls OpenAI, saves response.
    /// </summary>
    Task<ChatResponseDto> AskQuestionAsync(ChatRequestDto request, string userId);

    /// <summary>
    /// Regenerates an AI response for an existing question.
    /// Creates a new AIResponse linked to the same UserRequest.
    /// </summary>
    Task<ChatResponseDto> RegenerateResponseAsync(int userRequestId, string userId);
}

using EduAssist.AngularWebApi.DTOs;

namespace EduAssist.AngularWebApi.Services;

public interface IChatService
{
    Task<ChatResponseDto> AskQuestionAsync(ChatRequestDto request, string userId);
    Task<ChatResponseDto> RegenerateResponseAsync(int userRequestId, string userId);
}

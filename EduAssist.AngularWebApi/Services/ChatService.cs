using EduAssist.AngularWebApi.Context;
using EduAssist.AngularWebApi.DTOs;
using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _appDb;
    private readonly AuthDbContext _authDb;
    private readonly IOpenAIService _openAI;

    public ChatService(AppDbContext appDb, AuthDbContext authDb, IOpenAIService openAI)
    {
        _appDb = appDb; _authDb = authDb; _openAI = openAI;
    }

    public async Task<ChatResponseDto> AskQuestionAsync(ChatRequestDto request, string userId)
    {
        var category = await _appDb.Categories.FindAsync(request.CategoryId)
            ?? throw new InvalidOperationException("Invalid category.");
        var user = await _authDb.Users.FindAsync(userId);

        var userRequest = new UserRequest
        {
            Query = request.Query, CategoryId = request.CategoryId,
            UserId = userId, RequestedOn = DateTime.UtcNow
        };
        _appDb.UserRequests.Add(userRequest);
        await _appDb.SaveChangesAsync();

        string aiText;
        try { aiText = await _openAI.GetResponseAsync(request.Query, category.SubjectName, user?.Grade); }
        catch
        {
            throw new InvalidOperationException(
                "AI service temporarily unavailable. Your question was saved. Try regenerating later.");
        }

        var aiResponse = new AIResponse
        {
            Response = aiText, CreatedAt = DateTime.UtcNow, UserRequestId = userRequest.UserRequestId
        };
        _appDb.AIResponses.Add(aiResponse);

        if (user != null) { user.TotalQueriesAsked++; _authDb.Users.Update(user); await _authDb.SaveChangesAsync(); }
        await _appDb.SaveChangesAsync();

        return new ChatResponseDto
        {
            UserRequestId = userRequest.UserRequestId, AIResponseId = aiResponse.AIResponseId,
            Query = userRequest.Query, Response = aiResponse.Response,
            SubjectName = category.SubjectName, CreatedAt = aiResponse.CreatedAt
        };
    }


    public async Task<ChatResponseDto> RegenerateResponseAsync(int userRequestId, string userId)
    {
        var userRequest = await _appDb.UserRequests.Include(ur => ur.Category)
            .FirstOrDefaultAsync(ur => ur.UserRequestId == userRequestId && ur.UserId == userId)
            ?? throw new InvalidOperationException("Question not found.");

        var user = await _authDb.Users.FindAsync(userId);
        var aiText = await _openAI.GetResponseAsync(userRequest.Query, userRequest.Category.SubjectName, user?.Grade);

        var aiResponse = new AIResponse
        {
            Response = aiText, CreatedAt = DateTime.UtcNow, UserRequestId = userRequestId
        };
        _appDb.AIResponses.Add(aiResponse);
        await _appDb.SaveChangesAsync();

        return new ChatResponseDto
        {
            UserRequestId = userRequest.UserRequestId, AIResponseId = aiResponse.AIResponseId,
            Query = userRequest.Query, Response = aiResponse.Response,
            SubjectName = userRequest.Category.SubjectName, CreatedAt = aiResponse.CreatedAt
        };
    }
}

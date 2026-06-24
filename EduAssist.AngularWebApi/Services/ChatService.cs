using EduAssist.AngularWebApi.DTOs;
using EduAssist.EFCore.Context;
using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// Chat Service - Orchestrates the AI Q&A flow.
/// 
/// Flow:
/// 1. Save UserRequest to Core DB
/// 2. Get subject name for prompt context
/// 3. Call OpenAI with context-aware prompt
/// 4. Save AIResponse to Core DB
/// 5. Return formatted response to controller
/// 
/// On OpenAI failure: UserRequest is still saved, error is thrown for retry later.
/// </summary>
public class ChatService : IChatService
{
    private readonly AppDbContext _appDbContext;
    private readonly AuthDbContext _authDbContext;
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        AppDbContext appDbContext,
        AuthDbContext authDbContext,
        IOpenAIService openAIService,
        ILogger<ChatService> logger)
    {
        _appDbContext = appDbContext;
        _authDbContext = authDbContext;
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Processes a new question from a student.
    /// </summary>
    public async Task<ChatResponseDto> AskQuestionAsync(ChatRequestDto request, string userId)
    {
        // Validate category exists
        var category = await _appDbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

        if (category == null)
        {
            throw new InvalidOperationException("Invalid category selected.");
        }

        // Get user's grade for prompt context (cross-database query)
        var user = await _authDbContext.Users.FindAsync(userId);
        var grade = user?.Grade;

        // Save UserRequest first (even if AI fails, we keep the question)
        var userRequest = new UserRequest
        {
            Query = request.Query,
            CategoryId = request.CategoryId,
            UserId = userId,
            RequestedOn = DateTime.UtcNow
        };

        _appDbContext.UserRequests.Add(userRequest);
        await _appDbContext.SaveChangesAsync();

        // Call OpenAI
        string aiResponseText;
        try
        {
            aiResponseText = await _openAIService.GetResponseAsync(
                request.Query, category.SubjectName, grade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI failed for UserRequestId: {Id}", userRequest.UserRequestId);
            throw new InvalidOperationException(
                "AI service is temporarily unavailable. Your question has been saved. Please try regenerating later.");
        }

        // Save AI Response
        var aiResponse = new AIResponse
        {
            Response = aiResponseText,
            CreatedAt = DateTime.UtcNow,
            UserRequestId = userRequest.UserRequestId
        };

        _appDbContext.AIResponses.Add(aiResponse);

        // Update user's total queries count
        if (user != null)
        {
            user.TotalQueriesAsked += 1;
            _authDbContext.Users.Update(user);
            await _authDbContext.SaveChangesAsync();
        }

        await _appDbContext.SaveChangesAsync();

        return new ChatResponseDto
        {
            UserRequestId = userRequest.UserRequestId,
            AIResponseId = aiResponse.AIResponseId,
            Query = userRequest.Query,
            Response = aiResponse.Response,
            SubjectName = category.SubjectName,
            CreatedAt = aiResponse.CreatedAt
        };
    }

    /// <summary>
    /// Regenerates a new AI response for an existing question.
    /// Adds a new AIResponse record (1:Many relationship).
    /// </summary>
    public async Task<ChatResponseDto> RegenerateResponseAsync(int userRequestId, string userId)
    {
        // Validate the request belongs to this user
        var userRequest = await _appDbContext.UserRequests
            .Include(ur => ur.Category)
            .FirstOrDefaultAsync(ur => ur.UserRequestId == userRequestId && ur.UserId == userId);

        if (userRequest == null)
        {
            throw new InvalidOperationException("Question not found or you don't have permission to access it.");
        }

        // Get user's grade for prompt context
        var user = await _authDbContext.Users.FindAsync(userId);
        var grade = user?.Grade;

        // Call OpenAI again
        var aiResponseText = await _openAIService.GetResponseAsync(
            userRequest.Query, userRequest.Category.SubjectName, grade);

        // Save new AI Response (adds to the 1:Many collection)
        var aiResponse = new AIResponse
        {
            Response = aiResponseText,
            CreatedAt = DateTime.UtcNow,
            UserRequestId = userRequestId
        };

        _appDbContext.AIResponses.Add(aiResponse);
        await _appDbContext.SaveChangesAsync();

        return new ChatResponseDto
        {
            UserRequestId = userRequest.UserRequestId,
            AIResponseId = aiResponse.AIResponseId,
            Query = userRequest.Query,
            Response = aiResponse.Response,
            SubjectName = userRequest.Category.SubjectName,
            CreatedAt = aiResponse.CreatedAt
        };
    }
}

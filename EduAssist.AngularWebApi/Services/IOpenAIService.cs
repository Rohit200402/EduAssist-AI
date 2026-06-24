namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// Interface for OpenAI API integration.
/// Handles sending prompts to OpenAI and receiving AI-generated responses.
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Sends a student's question to OpenAI with subject context.
    /// </summary>
    /// <param name="query">The student's question.</param>
    /// <param name="subjectName">The academic subject for context.</param>
    /// <param name="grade">The student's grade level (optional, for response tailoring).</param>
    /// <returns>The AI-generated explanation text.</returns>
    Task<string> GetResponseAsync(string query, string subjectName, string? grade = null);
}

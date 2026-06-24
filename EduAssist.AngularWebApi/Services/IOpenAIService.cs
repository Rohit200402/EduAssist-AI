namespace EduAssist.AngularWebApi.Services;

public interface IOpenAIService
{
    Task<string> GetResponseAsync(string query, string subjectName, string? grade = null);
}

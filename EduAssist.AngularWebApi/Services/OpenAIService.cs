using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EduAssist.AngularWebApi.Services;

/// <summary>
/// OpenAI Service - Integrates with OpenAI REST API to generate educational responses.
/// Constructs context-aware prompts using the student's subject and grade level.
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _logger = logger;
    }

    /// <summary>
    /// Sends a question to OpenAI with subject context and returns the AI explanation.
    /// 
    /// Prompt structure:
    /// - System message: "You are an education assistant specializing in {subject}..."
    /// - User message: The student's actual question
    /// </summary>
    public async Task<string> GetResponseAsync(string query, string subjectName, string? grade = null)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        var maxTokens = int.Parse(_configuration["OpenAI:MaxTokens"] ?? "1000");

        if (string.IsNullOrEmpty(apiKey) || apiKey == "REPLACE_WITH_YOUR_OPENAI_KEY")
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured. Please add your key to appsettings.json.");
        }

        // Build context-aware system prompt
        var systemPrompt = BuildSystemPrompt(subjectName, grade);

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = query }
            },
            max_tokens = maxTokens,
            temperature = 0.7
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException(
                    $"OpenAI API returned {response.StatusCode}. Please try again later.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseBody);

            // Extract the assistant's message content
            var aiMessage = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return aiMessage ?? "No response generated. Please try again.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to OpenAI API.");
            throw new InvalidOperationException(
                "AI service is temporarily unavailable. Your question has been saved. Please try regenerating later.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling OpenAI API.");
            throw new InvalidOperationException(
                "An unexpected error occurred while generating the AI response. Please try again.", ex);
        }
    }

    /// <summary>
    /// Builds a context-aware system prompt based on the subject and student's grade.
    /// This helps OpenAI tailor the explanation to the student's level.
    /// </summary>
    private static string BuildSystemPrompt(string subjectName, string? grade)
    {
        var gradeContext = string.IsNullOrEmpty(grade)
            ? "a student"
            : $"a {grade} student";

        return $"""
            You are an expert education assistant specializing in {subjectName}.
            Your role is to explain concepts clearly and thoroughly to {gradeContext}.
            
            Guidelines:
            - Provide clear, structured explanations
            - Use simple language appropriate for the student's level
            - Include relevant examples and analogies
            - Break complex topics into smaller, understandable parts
            - If the question is unclear, provide the most helpful interpretation
            - Use bullet points or numbered lists for step-by-step explanations
            - End with a brief summary if the explanation is long
            """;
    }
}

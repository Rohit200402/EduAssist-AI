using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EduAssist.AngularWebApi.Services;

public class OpenAIService : IOpenAIService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public OpenAIService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClient = httpClientFactory.CreateClient("OpenAI");
    }

    public async Task<string> GetResponseAsync(string query, string subjectName, string? grade = null)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var model = _config["OpenAI:Model"] ?? "gpt-3.5-turbo";
        var maxTokens = int.Parse(_config["OpenAI:MaxTokens"] ?? "1000");

        if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("REPLACE"))
            throw new InvalidOperationException("OpenAI API key is not configured.");

        var gradeCtx = string.IsNullOrEmpty(grade) ? "a student" : $"a {grade} student";
        var systemPrompt = $"You are an expert education assistant specializing in {subjectName}. " +
            $"Explain concepts clearly to {gradeCtx}. Use examples and structured explanations.";

        var body = new
        {
            model, max_tokens = maxTokens, temperature = 0.7,
            messages = new[] {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = query }
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenAI returned {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseBody);
        return jsonDoc.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString()
            ?? "No response generated.";
    }
}

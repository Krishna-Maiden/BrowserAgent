using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AgentCore.Models;

namespace AgentCore.AI;

public class LlmProcessor
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<LlmProcessor> _logger;

    public LlmProcessor(HttpClient httpClient, IConfiguration config, ILogger<LlmProcessor> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<TaskPlan?> GeneratePlanAsync(string userInput)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var endpoint = "https://api.openai.com/v1/chat/completions";

        var prompt = LoadPromptFromFile();
        var requestBody = new
        {
            model = "gpt-4-0613",
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = userInput }
            }
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI request failed: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(responseJson);
        var content = result.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<TaskPlan>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize LLM response: {Content}", content);
            return null;
        }
    }

    private string LoadPromptFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "AgentCore", "AI", "LlmPrompt.txt");
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }
}
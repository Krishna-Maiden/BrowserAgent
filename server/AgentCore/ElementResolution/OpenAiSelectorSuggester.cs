using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public class OpenAiSelectorSuggester : ILlmSelectorSuggester
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAiSelectorSuggester(HttpClient client, IConfiguration config)
        {
            _httpClient = client;
            _apiKey = config["OpenAI:ApiKey"]; // <-- appsettings.json key
        }

        public async Task<string> SuggestSelectorAsync(IPage page, string logicalName)
        {
            var html = await page.ContentAsync();
            var request = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are an expert UI tester." },
                    new { role = "user", content = $"Suggest the best XPath for an element named '{logicalName}' from the following HTML: {html}" }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
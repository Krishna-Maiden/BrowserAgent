using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using AgentCore.Models;
using AgentCore.ElementResolution;

namespace AgentCore.ElementResolution
{
    public class OpenAiSelectorSuggester : ILlmSelectorSuggester
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _promptTemplate;

        public OpenAiSelectorSuggester(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _promptTemplate = File.ReadAllText("Prompts/llm_prompt_template.txt");
        }

        public async Task<string> SuggestSelectorAsync(IPage page, string logicalName)
        {
            string html = await page.ContentAsync();
            string prompt = _promptTemplate
                .Replace("{logicalName}", logicalName)
                .Replace("{html}", StripHeadTag(html));

            var requestBody = new
            {
                model = "gpt-4-0613",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        private string StripHeadTag(string html)
        {
            int headStart = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
            int headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headStart >= 0 && headEnd > headStart)
                return html.Remove(headStart, headEnd - headStart + 7);
            return html;
        }
    }
}
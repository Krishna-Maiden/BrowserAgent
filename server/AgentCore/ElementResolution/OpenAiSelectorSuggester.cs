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
                model = "gpt-4.1",
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

        private async Task<string> CallOpenAiWithRetryAsync(HttpClient client, HttpRequestMessage request)
        {
            const int maxRetries = 5;
            int delay = 1000;

            for (int i = 0; i < maxRetries; i++)
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                if ((int)response.StatusCode == 429)
                {
                    await Task.Delay(delay);
                    delay *= 2;
                    continue;
                }

                response.EnsureSuccessStatusCode(); // throw on other errors
            }

            throw new Exception("Max retry attempts exceeded for OpenAI API.");
        }

    }
}
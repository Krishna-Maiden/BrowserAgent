using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Playwright;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            var endpoint = "https://api.openai.com/v1/chat/completions";
            string rawHtml = await page.ContentAsync();
            string strippedHtml = HtmlCleaner.RemoveHeadTag(rawHtml); // new helper method
            var prompt = "You are an expert UI tester.";
            string userInput = $"Suggest the best XPath for an element named '{logicalName}' from the following HTML: {strippedHtml}";

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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        public static class HtmlCleaner
        {
            public static string RemoveHeadTag(string html)
            {
                if (string.IsNullOrWhiteSpace(html))
                    return html;

                int headStart = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
                if (headStart == -1)
                    return html;

                int headEnd = html.IndexOf("</head>", headStart, StringComparison.OrdinalIgnoreCase);
                if (headEnd == -1)
                    return html;

                headEnd += "</head>".Length;
                return html.Remove(headStart, headEnd - headStart);
            }
        }
    }
}
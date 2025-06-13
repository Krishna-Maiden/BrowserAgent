using System.Net.Http;
using System.Text.Json;
using System.Text;
using AgentCore.Models;

namespace AgentCore.AI
{
    public class LlmProcessor
    {
        private readonly HttpClient _httpClient = new();

        public async Task<List<TaskStep>> GeneratePlan(string input)
        {
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[] {
                    new { role = "system", content = "You convert user tasks into JSON-based browser action plans." },
                    new { role = "user", content = input }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", "Bearer sk-REPLACE_WITH_YOUR_KEY");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<TaskStep>>(content) ?? new List<TaskStep>();
        }
    }
}
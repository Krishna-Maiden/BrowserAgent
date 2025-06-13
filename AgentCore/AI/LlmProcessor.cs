using System.Net.Http;
using System.Text.Json;
using System.Text;
using AgentCore.Models;

namespace AgentCore.AI
{
    public class LlmProcessor
    {
        private readonly HttpClient _httpClient;

        public LlmProcessor()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<TaskStep>> GeneratePlan(string input)
        {
            // Replace with actual OpenAI endpoint and key
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[] {
                    new { role = "system", content = "You convert user tasks into JSON-based browser action plans." },
                    new { role = "user", content = input }
                }
            };

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            );

            var content = await response.Content.ReadAsStringAsync();
            var plan = JsonSerializer.Deserialize<List<TaskStep>>(content);
            return plan ?? new List<TaskStep>();
        }
    }
}
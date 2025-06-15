using AgentCore.ElementResolution;
using AgentCore.Models;
using Microsoft.Playwright;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class LocalLlmSelectorSuggester : ILlmSelectorSuggester
{
    private readonly string _model;
    private readonly string _promptTemplate;

    public LocalLlmSelectorSuggester(IConfiguration config)
    {
        _model = config.GetValue<string>("LlmConfig:OllamaModel") ?? "phi3:mini";
        _promptTemplate = File.ReadAllText("Prompts/llm_prompt_template.txt");
    }

    public async Task<string> SuggestSelectorAsync(IPage page, string logicalName)
    {
        string html = await page.ContentAsync();
        string prompt = _promptTemplate
            .Replace("{logicalName}", logicalName)
            .Replace("{html}", StripHeadTag(html));

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = _model,
                prompt,
                stream = false
            }), Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("response").GetString()?.Trim() ?? "";
    }

    private string StripHeadTag(string html)
    {
        int headStart = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
        int headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headStart >= 0 && headEnd > headStart)
            return html.Remove(headStart, headEnd - headStart + 7);
        return html;
    }

    public async Task<string> SuggestSelectorAsync(Identification identification)
    {
        var prompt = $"Suggest a robust CSS selector for an HTML element identified as '{identification.Type}' with label '{identification.Value}'. Respond with only the selector string.";

        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = _model,
                prompt,
                stream = false
            }), Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("response").GetString()?.Trim() ?? "";
    }
}

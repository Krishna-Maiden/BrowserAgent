using AgentCore.ElementResolution;
using AgentCore.Models;
using Microsoft.Playwright;

public class HybridSelectorSuggester : ILlmSelectorSuggester
{
    private readonly ILlmSelectorSuggester _local;
    private readonly ILlmSelectorSuggester _remote;

    public HybridSelectorSuggester(ILlmSelectorSuggester local, ILlmSelectorSuggester remote)
    {
        _local = local;
        _remote = remote;
    }

    public async Task<string> SuggestSelectorAsync(IPage page, string logicalName)
    {
        try
        {
            var result = await _local.SuggestSelectorAsync(page, logicalName);
            if (!string.IsNullOrWhiteSpace(result)) return result;
        }
        catch { }

        return await _remote.SuggestSelectorAsync(page, logicalName);
    }
}

using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public class InMemoryPageElementCache : IPageElementCache
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();

        public async Task CachePageElementsAsync(IPage page, string pageUrl)
        {
            var elements = await page.QuerySelectorAllAsync("*");
            var logicalMap = new Dictionary<string, string>();

            foreach (var element in elements)
            {
                try
                {
                    string? tag = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                    string? name = await element.GetAttributeAsync("name");
                    string? placeholder = await element.GetAttributeAsync("placeholder");
                    string? type = await element.GetAttributeAsync("type");
                    string? innerText = await element.InnerTextAsync();

                    var logicalName = BuildLogicalName(tag, name, placeholder, type, innerText);
                    if (!string.IsNullOrWhiteSpace(logicalName))
                    {
                        var selector = await page.EvaluateAsync<string>("el => window.getSelector?.(el) || ''", element);
                        if (!string.IsNullOrWhiteSpace(selector))
                            logicalMap[logicalName.ToLower()] = selector;
                    }
                }
                catch
                {
                    // Ignore problematic elements
                }
            }

            _cache[pageUrl] = logicalMap;
        }

        public string GetCachedSelector(string pageUrl, string logicalName)
        {
            if (_cache.TryGetValue(pageUrl, out var map))
            {
                if (map.TryGetValue(logicalName.ToLower(), out var selector))
                    return selector;
            }
            return null;
        }

        public List<string> GetLogicalNames(string pageUrl)
        {
            if (_cache.TryGetValue(pageUrl, out var map))
                return map.Keys.ToList();
            return new List<string>();
        }

        private string BuildLogicalName(string? tag, string? name, string? placeholder, string? type, string? innerText)
        {
            return $"{tag}_{name ?? placeholder ?? type ?? innerText}".Replace(" ", "_").ToLower();
        }
    }
}

using Microsoft.Playwright;
using System.Text.Json;
using System.Threading.Tasks;
using AgentCore.ElementResolution;

namespace AgentCore.Helpers
{
    public class PageElementCacheLoader
    {
        private readonly IPageElementCache _cache;

        public PageElementCacheLoader(IPageElementCache cache)
        {
            _cache = cache;
        }

        public async Task CachePageElementsAsync(IPage page, string pageUrl)
        {
            var elements = await page.QuerySelectorAllAsync("*");

            foreach (var element in elements)
            {
                try
                {
                    string tag = await element.EvaluateAsync<string>("e => e.tagName?.toLowerCase()");
                    string name = await element.GetAttributeAsync("name");
                    string placeholder = await element.GetAttributeAsync("placeholder");
                    string type = await element.GetAttributeAsync("type");
                    string ariaLabel = await element.GetAttributeAsync("aria-label");
                    string labelText = await page.EvaluateAsync<string>(
                        @"el => {
                            const label = el.closest('label');
                            return label ? label.innerText : '';
                        }", element);

                    string logicalName = name ?? placeholder ?? ariaLabel ?? labelText;
                    logicalName = logicalName?.Trim();

                    if (!string.IsNullOrWhiteSpace(logicalName))
                    {
                        string selector = await element.EvaluateAsync<string>(
                            "el => el.tagName.toLowerCase() + (el.name ? `[name='${el.name}']` : '')");

                        _cache.Save(pageUrl, logicalName, selector);
                    }
                }
                catch
                {
                    // Skip elements that cause exceptions (e.g., stale nodes)
                }
            }
        }
    }
}

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using AgentCore.Models;
using AgentCore.ElementResolution;

namespace AgentCore.Automation
{
    public class SelectorResolver
    {
        private readonly ISelectorMemory _memory;
        private readonly ILlmSelectorSuggester _llm;
        private readonly IPageElementCache _cache;

        public SelectorResolver(
            ISelectorMemory memory,
            ILlmSelectorSuggester llm,
            IPageElementCache cache)
        {
            _memory = memory;
            _llm = llm;
            _cache = cache;
        }

        public async Task<ILocator> ResolveAsync(IPage page, Identification identification)
        {
            string key = identification.Type + ":" + identification.Value;
            var cachedSelector = _memory.Get(key);

            // 1. Check memory cache first
            if (!string.IsNullOrWhiteSpace(cachedSelector))
            {
                var locator = page.Locator(SanitizeSelector(cachedSelector));
                if (await locator.IsVisibleAsync())
                    return locator;
            }

            // 2. Check page element cache (based on page URL + logical name)
            if (page.Url is string pageUrl)
            {
                var pageCached = _cache.Get(pageUrl, identification.Value);
                if (!string.IsNullOrWhiteSpace(pageCached))
                {
                    var locator = page.Locator(SanitizeSelector(pageCached));
                    if (await locator.IsVisibleAsync())
                    {
                        _memory.Save(key, pageCached); // Promote to memory
                        return locator;
                    }
                }
            }

            // 3. Fallback to LLM (local/Ollama or OpenAI via HybridSuggester)
            string selector = await _llm.SuggestSelectorAsync(page, identification.Value);
            selector = SanitizeSelector(selector);
            _memory.Save(key, selector);
            return page.Locator(selector);
        }

        private string SanitizeSelector(string selector)
        {
            if (string.IsNullOrWhiteSpace(selector)) return selector;

            // Remove backticks and trim quotes
            selector = selector.Trim().Trim('`', '\"', '\'');

            // Replace double quotes inside selector
            selector = Regex.Replace(selector, "\"(.*?)\"", "'$1'");

            return selector;
        }
    }
}

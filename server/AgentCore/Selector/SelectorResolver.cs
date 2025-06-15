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

            // 1. Check memory cache
            var memSelector = _memory.Get(key);
            if (!string.IsNullOrWhiteSpace(memSelector))
            {
                var locator = page.Locator(SanitizeSelector(memSelector));
                if (await locator.IsVisibleAsync())
                    return locator;
            }

            // 2. Check page element cache
            var cachedSelector = _cache.GetCachedSelector(page.Url, identification.Value);
            if (!string.IsNullOrWhiteSpace(cachedSelector))
            {
                var locator = page.Locator(SanitizeSelector(cachedSelector));
                if (await locator.IsVisibleAsync())
                {
                    _memory.Save(key, cachedSelector); // promote to memory
                    return locator;
                }
            }

            // 3. Fallback to LLM suggestion (local or remote)
            string newSelector = await _llm.SuggestSelectorAsync(page, identification.Value);
            newSelector = SanitizeSelector(newSelector);
            _memory.Save(key, newSelector);
            return page.Locator(newSelector);
        }

        private string SanitizeSelector(string selector)
        {
            if (string.IsNullOrWhiteSpace(selector)) return selector;

            selector = selector.Trim().Trim('`', '\"', '\'');
            return Regex.Replace(selector, "\"(.*?)\"", "'$1'");
        }
    }
}

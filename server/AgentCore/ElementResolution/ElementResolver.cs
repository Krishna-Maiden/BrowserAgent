using AgentCore.Automation;
using AgentCore.Models;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace AgentCore.ElementResolution
{
    public class ElementResolver
    {
        private readonly ISelectorMemory _memory;
        private readonly ILlmSelectorSuggester _llm;

        public ElementResolver(ISelectorMemory memory, ILlmSelectorSuggester llm)
        {
            _memory = memory;
            _llm = llm;
        }

        public async Task<ILocator> ResolveAsync(IPage page, Identification identification)
        {
            string key = identification.Type + ":" + identification.Value;
            var cachedSelector = _memory.Get(key);

            if (!string.IsNullOrWhiteSpace(cachedSelector))
            {
                var locator = page.Locator(cachedSelector);
                if (await locator.IsVisibleAsync())
                    return locator;
            }

            string selector = await _llm.SuggestSelectorAsync(page, identification.Value);

            _memory.Save(key, selector);
            return page.Locator(selector);
        }
    }
}

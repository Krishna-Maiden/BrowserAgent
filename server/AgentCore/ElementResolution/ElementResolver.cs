using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public class ElementResolver
    {
        private readonly ISelectorMemory _selectorMemory;
        private readonly ILlmSelectorSuggester _llmSuggester;

        public ElementResolver(ISelectorMemory memory, ILlmSelectorSuggester suggester)
        {
            _selectorMemory = memory;
            _llmSuggester = suggester;
        }

        public async Task<ILocator> ResolveSelectorAsync(IPage page, string logicalName)
        {
            var selector = _selectorMemory.GetSelector(logicalName);
            if (!string.IsNullOrEmpty(selector))
            {
                var locator = page.Locator(selector);
                if (await locator.CountAsync() > 0)
                    return locator;
            }

            // Ask LLM if no selector found or failed
            var suggested = await _llmSuggester.SuggestSelectorAsync(page, logicalName);
            if (!string.IsNullOrEmpty(suggested))
            {
                _selectorMemory.SaveSelector(logicalName, suggested);
                return page.Locator(suggested);
            }

            return null;
        }
    }
}
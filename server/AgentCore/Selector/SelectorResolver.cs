using AgentCore.ElementResolution;
using AgentCore.Models;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AgentCore.Automation
{
    public class SelectorResolver
    {
        private readonly ISelectorMemory _memory;
        private readonly ILlmSelectorSuggester _llm;

        public SelectorResolver(ISelectorMemory memory, ILlmSelectorSuggester llm)
        {
            _memory = memory;
            _llm = llm;
        }

        public async Task<string> ResolveAsync(IPage page, Identification identification)
        {
            if (identification == null || string.IsNullOrWhiteSpace(identification.Value))
                throw new ArgumentException("Identification value cannot be null.");

            // Check memory first
            var knownSelector = _memory.Get(identification.Value);
            if (!string.IsNullOrWhiteSpace(knownSelector))
                return knownSelector;

            // Try original selector
            try
            {
                var locator = page.Locator(identification.Value);
                if (await locator.IsVisibleAsync())
                {
                    _memory.Save(identification.Value, identification.Value);
                    return identification.Value;
                }
            }
            catch
            {
                // Continue to AI suggestion
            }

            // Fallback to LLM suggestion
            var suggestion = await _llm.SuggestSelectorAsync(page, identification.Value);
            if (!string.IsNullOrWhiteSpace(suggestion))
            {
                var locator = page.Locator(suggestion);
                if (await locator.IsVisibleAsync())
                {
                    _memory.Save(identification.Value, suggestion);
                    return suggestion;
                }
            }

            throw new Exception($"Unable to resolve selector: {identification.Value}");
        }
    }
}

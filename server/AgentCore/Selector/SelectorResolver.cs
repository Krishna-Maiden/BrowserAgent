using AgentCore.ElementResolution;
using AgentCore.Models;
using Microsoft.Playwright;
using System;
using System.Text.RegularExpressions;
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
            var selector = await _llm.SuggestSelectorAsync(page, identification.Value);
            selector = SanitizeSelector(selector);
            if (!string.IsNullOrWhiteSpace(selector))
            {
                var locator = page.Locator(selector);
                if (await locator.IsVisibleAsync())
                {
                    _memory.Save(identification.Value, selector);
                    return selector;
                }
            }

            throw new Exception($"Unable to resolve selector: {identification.Value}");
        }

        private string SanitizeSelector(string selector)
        {
            if (string.IsNullOrWhiteSpace(selector)) return selector;

            // Remove backticks and trim any quotes
            selector = selector.Trim().Trim('`', '\"', '\'');

            // Replace double quotes with single quotes inside selector
            selector = Regex.Replace(selector, "\"(.*?)\"", "'$1'");

            return selector;
        }
    }
}

using System.Collections.Concurrent;
using Microsoft.Playwright;
using AgentCore.Models;

namespace AgentCore.ElementResolution
{
    public class PageElementCache : IPageElementCache
    {
        // Cache keyed by pageUrl, then logicalName -> selector
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _cache = new();

        public async Task CachePageElementsAsync(IPage page, string pageUrl)
        {
            var elements = await page.EvaluateAsync(@"() => {
    const uiTags = ['input', 'button', 'select', 'textarea', 'a', 'label', 'div', 'span'];
    const nodes = Array.from(document.querySelectorAll('body *'));

    return nodes
        .filter(el => {
            const tag = el.tagName.toLowerCase();
            const isVisible = !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length);
            return uiTags.includes(tag) && isVisible;
        })
        .map(el => ({
            tag: el.tagName,
            text: el.innerText || '',
            attributes: {
                id: el.id || '',
                name: el.name || '',
                placeholder: el.placeholder || '',
                type: el.type || '',
                class: el.className || ''
            },
            selector: getUniqueSelector(el)
        }));

    function getUniqueSelector(el) {
        let path = [];
        while (el && el.nodeType === Node.ELEMENT_NODE) {
            let selector = el.nodeName.toLowerCase();
            if (el.id) {
                selector += '#' + el.id;
                path.unshift(selector);
                break;
            } else {
                let sib = el, nth = 1;
                while (sib = sib.previousElementSibling) {
                    if (sib.nodeName === el.nodeName) nth++;
                }
                selector += `:nth-of-type(${nth})`;
            }
            path.unshift(selector);
            el = el.parentNode;
        }
        return path.join(' > ');
    }
}");


            var logicalMap = new ConcurrentDictionary<string, string>();

            foreach (var element in elements)
            {
                string selector = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");

                var attrs = await element.EvaluateAsync<Dictionary<string, string>>(
                    @"el => {
                        const attrs = {};
                        for (const attr of el.attributes) {
                            attrs[attr.name] = attr.value;
                        }
                        return attrs;
                    }");

                string text = await element.InnerTextAsync();
                string logicalName = GenerateLogicalName(selector, attrs, text);

                if (!string.IsNullOrWhiteSpace(logicalName) && !logicalMap.ContainsKey(logicalName))
                {
                    var uniqueSelector = await page.EvaluateAsync<string>("el => el.outerHTML", element);
                    logicalMap[logicalName] = await element.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                }
            }

            _cache[pageUrl] = logicalMap;
        }

        public string GetCachedSelector(string pageUrl, string logicalName)
        {
            if (_cache.TryGetValue(pageUrl, out var pageSelectors) &&
                pageSelectors.TryGetValue(logicalName.ToLower(), out var selector))
            {
                return selector;
            }
            return null;
        }

        public List<string> GetLogicalNames(string pageUrl)
        {
            if (_cache.TryGetValue(pageUrl, out var pageSelectors))
            {
                return pageSelectors.Keys.ToList();
            }
            return new List<string>();
        }

        private string GenerateLogicalName(string tag, Dictionary<string, string> attrs, string text)
        {
            if (!string.IsNullOrWhiteSpace(text) && text.Length <= 100)
            {
                return text.Trim().ToLower();
            }

            if (attrs.TryGetValue("placeholder", out var placeholder))
            {
                return placeholder.ToLower();
            }

            if (attrs.TryGetValue("name", out var name))
            {
                return name.ToLower();
            }

            if (attrs.TryGetValue("id", out var id))
            {
                return id.ToLower();
            }

            return null;
        }
    }
}

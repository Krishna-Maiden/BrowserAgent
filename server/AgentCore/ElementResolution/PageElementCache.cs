using AgentCore.Models;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text.Json;

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
                        tag: el.tagName.toLowerCase(),
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

            var elementArray = elements?.GetRawText();
            if (elementArray == null) return;

            var list = JsonConvert.DeserializeObject<List<PageElementInfo>>(elementArray);

            var logicalMap = new ConcurrentDictionary<string, string>();
            foreach (var element in list)
            {
                var logicalName = GenerateLogicalName(element);
                if (!string.IsNullOrWhiteSpace(logicalName) && !logicalMap.ContainsKey(logicalName))
                {
                    logicalMap[logicalName] = element.Selector;
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

        private string GenerateLogicalName(PageElementInfo info)
        {
            try
            {
                if (info.Attributes == null)
                    return string.Empty;

                var json = JObject.FromObject(info.Attributes);

                var id = json.Value<string>("id");
                var name = json.Value<string>("name");
                var type = json.Value<string>("type");
                var placeholder = json.Value<string>("placeholder");
                var label = json.Value<string>("aria-label");
                var innerText = json.Value<string>("innerText");

                return $"{info.Tag}-{id ?? name ?? placeholder ?? label ?? innerText ?? type ?? "unknown"}"
                    .ToLowerInvariant()
                    .Replace(" ", "-");
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GenerateLogicalName3(PageElementInfo info)
        {
            try
            {
                if(info.Attributes == null)
                    return string.Empty;
                using var doc = JsonDocument.Parse(info.Attributes == null ? "{}": info.Attributes.ToString());
                var root = doc.RootElement;

                var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                var type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
                var placeholder = root.TryGetProperty("placeholder", out var placeholderProp) ? placeholderProp.GetString() : null;
                var label = root.TryGetProperty("aria-label", out var labelProp) ? labelProp.GetString() : null;
                var innerText = root.TryGetProperty("innerText", out var textProp) ? textProp.GetString() : null;

                return $"{info.Tag}-{id ?? name ?? placeholder ?? label ?? innerText ?? type ?? "unknown"}"
                    .ToLowerInvariant()
                    .Replace(" ", "-");
            }
            catch
            {
                return string.Empty; // Fallback in case of parsing issues
            }
        }

        private string GenerateLogicalName2(PageElementInfo element)
        {
            if (!string.IsNullOrWhiteSpace(element.Text) && element.Text.Length <= 100)
                return element.Text.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(element.Attributes.Placeholder))
                return element.Attributes.Placeholder.ToLower();

            if (!string.IsNullOrWhiteSpace(element.Attributes.Name))
                return element.Attributes.Name.ToLower();

            if (!string.IsNullOrWhiteSpace(element.Attributes.Id))
                return element.Attributes.Id.ToLower();

            return null;
        }

        public class PageElementInfo
        {
            public string Tag { get; set; }
            public string Text { get; set; }
            public string Selector { get; set; }
            public ElementAttributes Attributes { get; set; }
        }

        public class ElementAttributes
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Placeholder { get; set; }
            public string Type { get; set; }
            public string Class { get; set; }
        }
    }
}

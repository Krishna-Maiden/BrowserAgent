using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AgentCore.ElementResolution
{
    public class InMemoryPageElementCache : IPageElementCache
    {
        // Key format: pageUrl:logicalName -> selector
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public void Save(string pageUrl, string logicalName, string selector)
        {
            if (string.IsNullOrWhiteSpace(pageUrl) || string.IsNullOrWhiteSpace(logicalName) || string.IsNullOrWhiteSpace(selector))
                return;

            var key = BuildKey(pageUrl, logicalName);
            _cache[key] = selector;
        }

        public string Get(string pageUrl, string logicalName)
        {
            if (string.IsNullOrWhiteSpace(pageUrl) || string.IsNullOrWhiteSpace(logicalName))
                return null;

            var key = BuildKey(pageUrl, logicalName);
            return _cache.TryGetValue(key, out var selector) ? selector : null;
        }

        public void Clear(string pageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
            {
                _cache.Clear();
            }
            else
            {
                var prefix = $"{pageUrl.Trim().ToLowerInvariant()}::";
                foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix)).ToList())
                    _cache.TryRemove(key, out _);
            }
        }

        public IEnumerable<string> GetLogicalNames(string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(pageUrl)) return Enumerable.Empty<string>();

            var prefix = $"{pageUrl.Trim().ToLowerInvariant()}::";
            return _cache.Keys
                .Where(k => k.StartsWith(prefix))
                .Select(k => k.Substring(prefix.Length))
                .ToList();
        }

        private string BuildKey(string pageUrl, string logicalName)
        {
            return $"{pageUrl.Trim().ToLowerInvariant()}::{logicalName.Trim().ToLowerInvariant()}";
        }
    }
}

using System.Collections.Concurrent;
using AgentCore.Models;

namespace AgentCore.ElementResolution
{
    public class PageElementCache : IPageElementCache
    {
        private readonly ConcurrentDictionary<string, string> _cache = new();

        private string GetKey(string pageUrl, string logicalName) =>
            $"{pageUrl.ToLowerInvariant().Trim()}::{logicalName.ToLowerInvariant().Trim()}";

        public void Save(string pageUrl, string logicalName, string selector)
        {
            var key = GetKey(pageUrl, logicalName);
            _cache[key] = selector;
        }

        public string? Get(string pageUrl, string logicalName)
        {
            var key = GetKey(pageUrl, logicalName);
            return _cache.TryGetValue(key, out var selector) ? selector : null;
        }

        public void Clear(string pageUrl)
        {
            var prefix = pageUrl.ToLowerInvariant().Trim() + "::";
            foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix)))
            {
                _cache.TryRemove(key, out _);
            }
        }

        public IReadOnlyDictionary<string, string> GetAllForPage(string pageUrl)
        {
            var prefix = pageUrl.ToLowerInvariant().Trim() + "::";
            return _cache
                .Where(kvp => kvp.Key.StartsWith(prefix))
                .ToDictionary(
                    kvp => kvp.Key.Substring(prefix.Length),
                    kvp => kvp.Value
                );
        }
    }
}

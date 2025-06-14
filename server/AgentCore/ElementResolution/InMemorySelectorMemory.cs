using System.Collections.Concurrent;

namespace AgentCore.ElementResolution
{
    public class InMemorySelectorMemory : ISelectorMemory
    {
        private readonly ConcurrentDictionary<string, string> _selectors = new();

        public string GetSelector(string logicalName)
        {
            _selectors.TryGetValue(logicalName, out var selector);
            return selector;
        }

        public void SaveSelector(string logicalName, string selector)
        {
            _selectors[logicalName] = selector;
        }
    }
}
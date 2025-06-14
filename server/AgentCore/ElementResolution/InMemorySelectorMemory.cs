using AgentCore.Automation;
using System.Collections.Concurrent;

namespace AgentCore.ElementResolution
{
    /// <summary>
    /// Simple in-memory implementation of ISelectorMemory.
    /// </summary>
    public class InMemorySelectorMemory : ISelectorMemory
    {
        private readonly ConcurrentDictionary<string, string> _memory = new();

        public void Save(string key, string selector)
        {
            _memory[key] = selector;
        }

        public string? Get(string key)
        {
            _memory.TryGetValue(key, out var selector);
            return selector;
        }
    }
}

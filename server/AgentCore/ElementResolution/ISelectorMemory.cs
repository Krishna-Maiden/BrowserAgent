namespace AgentCore.Automation
{
    /// <summary>
    /// Interface for storing and retrieving resolved selectors.
    /// This allows selector caching to improve performance and support self-healing.
    /// </summary>
    public interface ISelectorMemory
    {
        /// <summary>
        /// Retrieves a previously resolved selector for a given identifier.
        /// </summary>
        /// <param name="key">The key or natural language description of the element.</param>
        /// <returns>The resolved selector if available; otherwise, null.</returns>
        string? Get(string key);

        /// <summary>
        /// Saves a resolved selector to memory.
        /// </summary>
        /// <param name="key">The key or natural language description.</param>
        /// <param name="selector">The resolved selector string.</param>
        void Save(string key, string selector);
    }
}

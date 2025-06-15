using System.Collections.Generic;

namespace AgentCore.ElementResolution
{
    public interface IPageElementCache
    {
        /// <summary>
        /// Saves a selector for a given page URL and logical name.
        /// </summary>
        void Save(string pageUrl, string logicalName, string selector);

        /// <summary>
        /// Retrieves a selector if available for the given page and logical name.
        /// </summary>
        string Get(string pageUrl, string logicalName);

        /// <summary>
        /// Clears the entire cache or for a specific page.
        /// </summary>
        void Clear(string pageUrl = null);

        /// <summary>
        /// Lists all known logical names for a given page.
        /// </summary>
        IEnumerable<string> GetLogicalNames(string pageUrl);
    }
}

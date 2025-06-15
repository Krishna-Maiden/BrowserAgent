using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public interface IPageElementCache
    {
        Task CachePageElementsAsync(IPage page, string pageUrl);
        string GetCachedSelector(string pageUrl, string logicalName);
        List<string> GetLogicalNames(string pageUrl);
    }
}

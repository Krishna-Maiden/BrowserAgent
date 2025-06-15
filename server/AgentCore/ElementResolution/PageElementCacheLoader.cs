using System.Threading.Tasks;
using AgentCore.ElementResolution;
using Microsoft.Playwright;

namespace server.AgentCore.ElementResolution
{
    public class PageElementCacheLoader
    {
        private readonly IPageElementCache _cache;

        public PageElementCacheLoader(IPageElementCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// This should be called right after page navigation to cache the elements.
        /// </summary>
        public async Task LoadAsync(IPage page)
        {
            var url = page.Url;
            if (!string.IsNullOrEmpty(url))
            {
                await _cache.CachePageElementsAsync(page, url);
            }
        }
    }
}

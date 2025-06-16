using System.Threading.Tasks;
using AgentCore.ElementResolution;
using Microsoft.Playwright;

namespace AgentCore.ElementResolution
{
    public class PageElementCacheLoader
    {
        private readonly IPageElementCache _cache;

        public PageElementCacheLoader(IPageElementCache cache)
        {
            _cache = cache;
        }

        public async Task LoadAsync(IPage page)
        {
            try
            {
                // Ensure the page is fully loaded before capturing
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                var pageUrl = page.Url;
                if (!string.IsNullOrEmpty(pageUrl))
                {
                    await _cache.CachePageElementsAsync(page, pageUrl);
                }
            }
            catch (System.Exception ex)
            {
                // Optionally log or handle exception
                System.Console.WriteLine($"[CacheLoader] Failed to cache page elements: {ex.Message}");
            }
        }
    }
}

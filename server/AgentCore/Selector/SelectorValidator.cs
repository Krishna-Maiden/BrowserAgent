
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentCore.Selector
{
    public class SelectorValidator
    {
        public static async Task<string> GetValidSelectorAsync(IPage page, List<string> selectors)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var element = await page.QuerySelectorAsync(selector);
                    if (element != null)
                        return selector;
                }
                catch
                {
                    continue;
                }
            }
            return null;
        }
    }
}

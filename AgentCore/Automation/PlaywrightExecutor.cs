using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using AgentCore.Models;

namespace AgentCore.Automation
{
    public class PlaywrightExecutor
    {
        public async Task ExecutePlanAsync(List<TaskStep> steps)
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var page = await browser.NewPageAsync();

            foreach (var step in steps)
            {
                switch (step.Action)
                {
                    case "navigate":
                        await page.GotoAsync(step.Url);
                        break;
                    case "type":
                        await page.FillAsync(step.Selector, step.Value);
                        break;
                    case "click":
                        await page.ClickAsync(step.Selector);
                        break;
                }
            }

            await browser.CloseAsync();
        }
    }
}
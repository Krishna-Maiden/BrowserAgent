using AgentCore.ElementResolution;
using AgentCore.Models;
using AgentCore.Selector;
using Microsoft.Playwright;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentCore.Automation
{
    public class PlaywrightExecutor
    {
        private readonly SelectorResolver _resolver;
        private readonly IPageElementCache _cache;

        public PlaywrightExecutor(SelectorResolver resolver, IPageElementCache cache)
        {
            _resolver = resolver;
            _cache = cache;
        }

        private string StripHeadTag(string html)
        {
            int headStart = html.IndexOf("<head", StringComparison.OrdinalIgnoreCase);
            int headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headStart >= 0 && headEnd > headStart)
                return html.Remove(headStart, headEnd - headStart + 7);
            return html;
        }

        public async Task ExecutePlanAsync(List<TaskStep> steps)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            foreach (var step in steps)
            {
                if (!string.IsNullOrWhiteSpace(step.Url))
                {
                    await page.GotoAsync(step.Url);
                    // Preload all components for this page
                    await _cache.CachePageElementsAsync(page, step.Url);
                }

                var selector = step.Identification != null
                    ? await _resolver.ResolveAsync(page, step.Identification)
                    : null;

                switch (step.Action)
                {
                    case "open_url":
                        if (!string.IsNullOrWhiteSpace(step.Url))
                            await page.GotoAsync(step.Url);
                        break;

                    case "login":
                        if (step.Value is JsonElement credentials &&
                            credentials.TryGetProperty("username", out var usernameEl) &&
                            credentials.TryGetProperty("password", out var passwordEl))
                        {
                            var username = usernameEl.GetString();
                            var password = passwordEl.GetString();

                            var usernameSelector = await _resolver.ResolveAsync(page, new Identification { Type = "logical", Value = "username" });
                            var passwordSelector = await _resolver.ResolveAsync(page, new Identification { Type = "logical", Value = "password" });
                            var loginButtonSelector = await _resolver.ResolveAsync(page, new Identification { Type = "logical", Value = "login button" });

                            await usernameSelector.FillAsync(username);
                            await passwordSelector.FillAsync(password);
                            await loginButtonSelector.ClickAsync();
                        }
                        break;

                    case "click":
                        await selector.ClickAsync();
                        break;

                    case "input_text":
                        await selector.FillAsync(step.Value.ToString());
                        break;

                    case "hover":
                        await selector.HoverAsync();
                        break;

                    //case "scroll_to":
                    //    if (!string.IsNullOrWhiteSpace(selector))
                    //        await page.Locator(selector).ScrollIntoViewIfNeededAsync();
                    //    break;

                    //case "assert_exists":
                    //    if (!string.IsNullOrWhiteSpace(selector))
                    //        await page.WaitForSelectorAsync(selector);
                    //    break;

                    //case "screenshot":
                    //    var path = step.Value?.ToString() ?? "screenshot.png";
                    //    await page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
                    //    break;

                    //case "wait":
                    //    if (step.TimeInSeconds.HasValue)
                    //        await Task.Delay(step.TimeInSeconds.Value * 1000);
                    //    break;

                    //case "wait_for_element":
                    //    if (!string.IsNullOrWhiteSpace(selector) && step.TimeInSeconds.HasValue)
                    //        await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = step.TimeInSeconds.Value * 1000 });
                    //    break;

                    default:
                        throw new Exception($"Unsupported action: {step.Action}");
                }
            }
        }
    }
}

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

        public PlaywrightExecutor(SelectorResolver resolver)
        {
            _resolver = resolver;
        }

        public async Task ExecutePlanAsync(List<TaskStep> steps)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            foreach (var step in steps)
            {
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
                        if (!string.IsNullOrWhiteSpace(step.Url))
                            await page.GotoAsync(step.Url);

                        if (step.Value is JsonElement credentials &&
                            credentials.TryGetProperty("username", out var usernameEl) &&
                            credentials.TryGetProperty("password", out var passwordEl))
                        {
                            var username = usernameEl.GetString();
                            var password = passwordEl.GetString();

                            var usernameLocator = await _resolver.ResolveAsync(page, new Identification
                            {
                                Type = "logicalName",
                                Value = "username"
                            });
                            await usernameLocator.FillAsync(username);

                            var passwordLocator = await _resolver.ResolveAsync(page, new Identification
                            {
                                Type = "logicalName",
                                Value = "password"
                            });
                            await passwordLocator.FillAsync(password);

                            var loginButtonLocator = await _resolver.ResolveAsync(page, new Identification
                            {
                                Type = "logicalName",
                                Value = "login"
                            });
                            await loginButtonLocator.ClickAsync();
                        }
                        break;

                    case "click":
                        if (!string.IsNullOrWhiteSpace(selector))
                            await page.ClickAsync(selector);
                        break;

                    case "input_text":
                        if (!string.IsNullOrWhiteSpace(selector) && step.Value != null)
                            await page.FillAsync(selector, step.Value.ToString());
                        break;

                    case "hover":
                        if (!string.IsNullOrWhiteSpace(selector))
                            await page.HoverAsync(selector);
                        break;

                    case "scroll_to":
                        if (!string.IsNullOrWhiteSpace(selector))
                            await page.Locator(selector).ScrollIntoViewIfNeededAsync();
                        break;

                    case "assert_exists":
                        if (!string.IsNullOrWhiteSpace(selector))
                            await page.WaitForSelectorAsync(selector);
                        break;

                    //case "assert_text":
                    //    if (!string.IsNullOrWhiteSpace(selector) && step.Value is string expectedText)
                    //    {
                    //        var content = await page.TextContentAsync(selector);
                    //        if (content == null || !content.Contains(expectedText))
                    //            throw new Exception("Text assertion failed.");
                    //    }
                    //    break;

                    case "screenshot":
                        var path = step.Value?.ToString() ?? "screenshot.png";
                        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
                        break;

                    //case "execute_script":
                    //    if (step.Value is string script)
                    //        await page.EvaluateAsync(script);
                    //    break;

                    //case "upload_file":
                    //    if (!string.IsNullOrWhiteSpace(selector) && step.Value is string filePath)
                    //        await page.SetInputFilesAsync(selector, filePath);
                    //    break;

                    case "wait":
                        if (step.TimeInSeconds.HasValue)
                            await Task.Delay(step.TimeInSeconds.Value * 1000);
                        break;

                    case "wait_for_element":
                        if (!string.IsNullOrWhiteSpace(selector) && step.TimeInSeconds.HasValue)
                            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { Timeout = step.TimeInSeconds.Value * 1000 });
                        break;

                    default:
                        throw new Exception($"Unsupported action: {step.Action}");
                }
            }
        }
    }
}
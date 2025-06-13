
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentCore.Models;
using Microsoft.Playwright;

namespace AgentCore.Automation
{
    public class PlaywrightExecutor
    {
        public async Task ExecutePlanAsync(List<TaskStep> steps)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            foreach (var step in steps)
            {
                try
                {
                    switch (step.Action)
                    {
                        case "open_url":
                            if (!string.IsNullOrWhiteSpace(step.Url))
                                await page.GotoAsync(step.Url);
                            break;

                        case "input_text":
                            if (step.Identification != null)
                                await page.FillAsync(step.Identification.Value, step.Value);
                            break;

                        case "click":
                            if (step.Identification != null)
                                await page.ClickAsync(step.Identification.Value);
                            break;

                        case "wait":
                            await Task.Delay(step.TimeInSeconds * 1000);
                            break;

                        case "hover":
                            if (step.Identification != null)
                                await page.HoverAsync(step.Identification.Value);
                            break;

                        case "press_key":
                            if (!string.IsNullOrWhiteSpace(step.Value))
                                await page.Keyboard.PressAsync(step.Value);
                            break;

                        case "scroll_to":
                            if (step.Identification != null)
                                await page.Locator(step.Identification.Value).ScrollIntoViewIfNeededAsync();
                            break;

                        case "assert_exists":
                            if (step.Identification != null)
                                await page.WaitForSelectorAsync(step.Identification.Value);
                            break;

                        case "assert_text":
                            if (step.Identification != null && !string.IsNullOrWhiteSpace(step.Value))
                            {
                                var content = await page.TextContentAsync(step.Identification.Value);
                                if (!content.Contains(step.Value))
                                    throw new Exception("Text assertion failed.");
                            }
                            break;

                        case "screenshot":
                            await page.ScreenshotAsync(new PageScreenshotOptions { Path = step.Value ?? "screenshot.png" });
                            break;

                        case "execute_script":
                            if (!string.IsNullOrWhiteSpace(step.Value))
                                await page.EvaluateAsync(step.Value);
                            break;

                        case "upload_file":
                            if (step.Identification != null && !string.IsNullOrWhiteSpace(step.Value))
                                await page.SetInputFilesAsync(step.Identification.Value, step.Value);
                            break;

                        case "wait_for_element":
                            if (step.Identification != null)
                                await page.WaitForSelectorAsync(step.Identification.Value, new PageWaitForSelectorOptions { Timeout = step.TimeInSeconds * 1000 });
                            break;

                        // Add additional actions as needed, keeping to the same format
                        default:
                            Console.WriteLine($"Unknown action: {step.Action}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing step {step.Description}: {ex.Message}");
                }
            }

            await browser.CloseAsync();
        }
    }
}

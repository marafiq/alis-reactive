using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public static class LocatorInteractionExtensions
{
    public static async Task ClickWhenStableAsync(this ILocator locator, IPage page, int timeoutMs = 60000)
    {
        await locator.ScrollIntoViewIfNeededAsync();

        try
        {
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine(
                $"ClickWhenStableAsync retrying after timeout for locator '{locator}': {ex.Message}");
            await locator.ScrollIntoViewIfNeededAsync();
            await page.WaitForTimeoutAsync(250);
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
    }
}

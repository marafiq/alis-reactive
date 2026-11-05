using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public static class LocatorInteractionExtensions
{
    public static async Task ClickWhenStableAsync(this ILocator locator, IPage page, int timeoutMs = 60000)
        => await locator.ClickAsync(new() { Timeout = timeoutMs });
}

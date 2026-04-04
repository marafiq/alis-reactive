using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal static class LocatorInteractionExtensions
{
    internal static async Task ClickWhenStableAsync(this ILocator locator, int timeoutMs = 60000)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        await locator.ClickAsync(new() { Timeout = timeoutMs });
    }

    internal static async Task ClickWithoutScrollingWhenStableAsync(
        this ILocator locator,
        int timeoutMs = 60000)
        => await locator.ClickAsync(new() { Timeout = timeoutMs });

    internal static async Task ClearWhenStableAsync(this ILocator locator, int timeoutMs = 60000)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        await locator.ClearAsync(new() { Timeout = timeoutMs });
    }
}

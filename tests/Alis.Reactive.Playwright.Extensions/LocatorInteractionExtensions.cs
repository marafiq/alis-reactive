using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public static class LocatorInteractionExtensions
{
    public static async Task ClickWhenStableAsync(this ILocator locator, IPage page, int timeoutMs = 60000)
        => await ClickCoreAsync(locator, page, timeoutMs, scrollIntoView: true,
            operationName: nameof(ClickWhenStableAsync));

    public static async Task ClickWithoutScrollingWhenStableAsync(
        this ILocator locator,
        IPage page,
        int timeoutMs = 60000)
        => await ClickCoreAsync(locator, page, timeoutMs, scrollIntoView: false,
            operationName: nameof(ClickWithoutScrollingWhenStableAsync));

    public static async Task ClearWhenStableAsync(this ILocator locator, IPage page, int timeoutMs = 60000)
        => await ClearCoreAsync(locator, page, timeoutMs, operationName: nameof(ClearWhenStableAsync));

    private static async Task ClickCoreAsync(
        ILocator locator,
        IPage page,
        int timeoutMs,
        bool scrollIntoView,
        string operationName)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            var remainingMs = (int)Math.Max(1000, (deadline - DateTime.UtcNow).TotalMilliseconds);
            var attemptTimeout = Math.Min(5000, remainingMs);

            try
            {
                if (scrollIntoView)
                    await locator.ScrollIntoViewIfNeededAsync();

                await locator.ClickAsync(new() { Timeout = attemptTimeout });
                return;
            }
            catch (TimeoutException ex)
            {
                lastError = ex;
                Console.WriteLine(
                    $"{operationName} retrying after timeout for locator '{locator}': {ex.Message}");
                await page.WaitForTimeoutAsync(250);
            }
            catch (PlaywrightException ex) when (IsTransientClickFailure(ex))
            {
                lastError = ex;
                Console.WriteLine(
                    $"{operationName} retrying after transient failure for locator '{locator}': {ex.Message}");
                await page.WaitForTimeoutAsync(150);
            }
        }

        throw lastError ?? new TimeoutException(
            $"{operationName} timed out after {timeoutMs}ms for locator '{locator}'.");
    }

    private static async Task ClearCoreAsync(
        ILocator locator,
        IPage page,
        int timeoutMs,
        string operationName)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            var remainingMs = (int)Math.Max(1000, (deadline - DateTime.UtcNow).TotalMilliseconds);
            var attemptTimeout = Math.Min(5000, remainingMs);

            try
            {
                await locator.ScrollIntoViewIfNeededAsync();
                await locator.ClearAsync(new() { Timeout = attemptTimeout });
                return;
            }
            catch (TimeoutException ex)
            {
                lastError = ex;
                Console.WriteLine(
                    $"{operationName} retrying after timeout for locator '{locator}': {ex.Message}");
                await page.WaitForTimeoutAsync(250);
            }
            catch (PlaywrightException ex) when (IsTransientInputFailure(ex))
            {
                lastError = ex;
                Console.WriteLine(
                    $"{operationName} retrying after transient failure for locator '{locator}': {ex.Message}");
                await page.WaitForTimeoutAsync(150);
            }
        }

        throw lastError ?? new TimeoutException(
            $"{operationName} timed out after {timeoutMs}ms for locator '{locator}'.");
    }

    private static bool IsTransientClickFailure(PlaywrightException ex)
    {
        return ex.Message.Contains("Element is not attached to the DOM", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("element was detached from the DOM", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("Element is not stable", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTransientInputFailure(PlaywrightException ex)
    {
        return IsTransientClickFailure(ex)
            || ex.Message.Contains("Element is not editable", StringComparison.OrdinalIgnoreCase);
    }
}

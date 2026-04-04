using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class DatePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DatePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    // ─── Gestures — What the User Does ───

    public async Task Fill(string dateText)
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateText)
    {
        await Fill(dateText);
        await Blur();
    }

    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickWhenStableAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(Popup, year, month);

        var dayCell = Popup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickWithoutScrollingWhenStableAsync();
    }

    // ─── Private Helpers ───

    private async Task NavigateToMonth(ILocator popup, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);
        var title = popup.Locator(".e-title");

        for (var i = 0; i < 24; i++) // max 2 years of navigation
        {
            var titleText = await title.TextContentAsync() ?? "";

            if (DateTime.TryParseExact(titleText.Trim(), "MMMM yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var current))
            {
                var currentMonth = new DateTime(current.Year, current.Month, 1);
                if (currentMonth == target)
                    return;

                if (target < currentMonth)
                    await popup.Locator(".e-prev").ClickWithoutScrollingWhenStableAsync();
                else
                    await popup.Locator(".e-next").ClickWithoutScrollingWhenStableAsync();

                await WaitForTitleChange(title, titleText.Trim());
            }
        }
    }

    private static async Task WaitForTitleChange(ILocator title, string previousText, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var currentText = (await title.TextContentAsync())?.Trim();
            if (!string.Equals(currentText, previousText, StringComparison.Ordinal))
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException($"Calendar title did not change from '{previousText}' within {timeoutMs}ms.");
    }
}

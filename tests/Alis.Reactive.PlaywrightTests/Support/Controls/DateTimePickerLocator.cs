using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class DateTimePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DateTimePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    public ILocator ClockIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-time-icon");

    public ILocator CalendarPopup => _page.Locator($"div#{_componentId}_options");

    public ILocator TimeListPopup => _page.Locator($"ul#{_componentId}_options");

    // ─── Gestures — What the User Does ───

    public async Task Fill(string dateTimeText)
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateTimeText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateTimeText)
    {
        await Fill(dateTimeText);
        await Blur();
    }

    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickWhenStableAsync();
        await CalendarPopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(CalendarPopup, year, month);

        var dayCell = CalendarPopup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickWhenStableAsync();
    }

    public async Task SelectTime(string timeText)
    {
        await ClockIcon.ClickWhenStableAsync();
        await TimeListPopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var item = TimeListPopup.Locator($".e-list-item[data-value='{timeText}']");
        await item.ClickWhenStableAsync();
    }

    public async Task Select(int year, int month, int day, string timeText)
    {
        await SelectDate(year, month, day);
        // Wait for calendar popup to fully close before opening time popup
        await CalendarPopup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        await SelectTime(timeText);
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
                    await popup.Locator(".e-prev").ClickWhenStableAsync();
                else
                    await popup.Locator(".e-next").ClickWhenStableAsync();

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

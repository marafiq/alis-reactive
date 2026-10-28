using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Uses popup gestures because typed text does not always update Syncfusion <c>ej2.value</c>.
/// </summary>
/// <remarks>
/// Calendar and time popups both use <c>#{id}_options</c>.
/// </remarks>
public sealed class DateTimePickerLocator
{
    private const int MaxMonthNavigationAttempts = 24;

    private readonly IPage _page;
    private readonly string _componentId;

    internal DateTimePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    public ILocator ClockIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-time-icon");

    /// <summary>Calendar popup; tag-qualified because Syncfusion reuses <c>#{id}_options</c>.</summary>
    public ILocator CalendarPopup => _page.Locator($"div#{_componentId}_options");

    /// <summary>Time list popup; tag-qualified because Syncfusion reuses <c>#{id}_options</c>.</summary>
    public ILocator TimeListPopup => _page.Locator($"ul#{_componentId}_options");

    public async Task Fill(string dateTimeText)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressSequentiallyAsync(dateTimeText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("ControlOrMeta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateTimeText)
    {
        await Fill(dateTimeText);
        await Blur();
    }

    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickWhenStableAsync(_page);
        await CalendarPopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(CalendarPopup, year, month);

        var dayCell = CalendarPopup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickWhenStableAsync(_page);
    }

    /// <summary>Selects a visible 30-minute interval, for example <c>8:00 AM</c>.</summary>
    public async Task SelectTime(string timeText)
    {
        await ClockIcon.ClickWhenStableAsync(_page);
        await TimeListPopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var item = TimeListPopup.Locator($".e-list-item[data-value='{timeText}']");
        await item.ClickWhenStableAsync(_page);
    }

    /// <summary>Selects date and time through popups, waiting between them to avoid popup collision.</summary>
    public async Task Select(int year, int month, int day, string timeText)
    {
        await SelectDate(year, month, day);
        await CalendarPopup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        await SelectTime(timeText);
    }

    private async Task NavigateToMonth(ILocator popup, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);
        var title = popup.Locator(".e-title");

        for (var navigationAttempt = 0; navigationAttempt < MaxMonthNavigationAttempts; navigationAttempt++)
        {
            var titleText = await title.TextContentAsync() ?? "";

            if (DateTime.TryParseExact(titleText.Trim(), "MMMM yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var current))
            {
                var currentMonth = new DateTime(current.Year, current.Month, 1);
                if (currentMonth == target)
                    return;

                if (target < currentMonth)
                    await popup.Locator(".e-prev").ClickWhenStableAsync(_page);
                else
                    await popup.Locator(".e-next").ClickWhenStableAsync(_page);

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

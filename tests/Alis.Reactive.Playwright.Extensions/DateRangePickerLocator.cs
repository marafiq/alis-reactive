using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Uses popup gestures because typed text does not always update Syncfusion range values.
/// </summary>
/// <remarks>
/// DateRangePicker uses <c>#{id}_popup</c>, unlike DatePicker and DateTimePicker.
/// </remarks>
public sealed class DateRangePickerLocator
{
    private const int MaxMonthNavigationAttempts = 24;

    private readonly IPage _page;
    private readonly string _componentId;

    internal DateRangePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator RangeIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-range-icon");

    /// <summary>Range popup; DateRangePicker uses <c>#{id}_popup</c>, not <c>#{id}_options</c>.</summary>
    public ILocator RangePopup => _page.Locator($"#{_componentId}_popup");

    public ILocator LeftCalendar => RangePopup.Locator(".e-left-container .e-calendar");

    public ILocator RightCalendar => RangePopup.Locator(".e-right-container .e-calendar");

    public ILocator ApplyButton => RangePopup.Locator("button.e-apply");

    public async Task Fill(string dateRangeText)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateRangeText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateRangeText)
    {
        await Fill(dateRangeText);
        await Blur();
    }

    /// <summary>Selects the start and end dates through Syncfusion calendars, then applies the range.</summary>
    public async Task SelectRange(int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay)
    {
        await RangeIcon.ClickWhenStableAsync(_page);
        await RangePopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        await NavigateCalendarToMonth(LeftCalendar, startYear, startMonth);
        await LeftCalendar
            .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{startDay}\")")
            .ClickWhenStableAsync(_page);

        var endTarget = new DateTime(endYear, endMonth, 1);
        var startTarget = new DateTime(startYear, startMonth, 1);

        if (endTarget == startTarget)
        {
            await LeftCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync(_page);
        }
        else if (endTarget == startTarget.AddMonths(1))
        {
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync(_page);
        }
        else
        {
            await NavigateCalendarToMonth(LeftCalendar, endTarget.AddMonths(-1).Year,
                endTarget.AddMonths(-1).Month);
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync(_page);
        }

        await ApplyButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        // TODO: Replace this fixed post-render pause with a Syncfusion Apply button stability signal.
        await _page.WaitForTimeoutAsync(150);
        await ApplyButton.ClickAsync(new() { Timeout = 5000 });
    }

    private async Task NavigateCalendarToMonth(ILocator calendar, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);
        var title = calendar.Locator(".e-title");

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
                    await calendar.Locator(".e-prev").ClickWhenStableAsync(_page);
                else
                    await calendar.Locator(".e-next").ClickWhenStableAsync(_page);

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

using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class DateRangePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DateRangePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator RangeIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-range-icon");

    public ILocator RangePopup => _page.Locator($"#{_componentId}_popup");

    public ILocator LeftCalendar => RangePopup.Locator(".e-left-container .e-calendar");

    public ILocator RightCalendar => RangePopup.Locator(".e-right-container .e-calendar");

    public ILocator ApplyButton => RangePopup.Locator("button.e-apply");

    // ─── Gestures — What the User Does ───

    public async Task Fill(string dateRangeText)
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateRangeText, new() { Delay = 30 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickWhenStableAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string dateRangeText)
    {
        await Fill(dateRangeText);
        await Blur();
    }

    public async Task SelectRange(int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay)
    {
        var previousValue = await Input.InputValueAsync();

        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
        await RangeIcon.ClickWhenStableAsync();
        await RangePopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        // Navigate left calendar to start month and click start day
        await NavigateCalendarToMonth(LeftCalendar, startYear, startMonth);
        await LeftCalendar
            .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{startDay}\")")
            .ClickWhenStableAsync();

        // After clicking start date, determine where to click end date.
        // The right calendar always shows left + 1 month.
        // Navigate left calendar so the end month is visible in either left or right.
        var endTarget = new DateTime(endYear, endMonth, 1);
        var startTarget = new DateTime(startYear, startMonth, 1);

        if (endTarget == startTarget)
        {
            // Same month — end day is also in left calendar
            await LeftCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync();
        }
        else if (endTarget == startTarget.AddMonths(1))
        {
            // End is in the right calendar (left + 1)
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync();
        }
        else
        {
            // End is further away — navigate left calendar to (endMonth - 1) so right shows endMonth
            await NavigateCalendarToMonth(LeftCalendar, endTarget.AddMonths(-1).Year,
                endTarget.AddMonths(-1).Month);
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickWhenStableAsync();
        }

        await ConfirmSelectionIfRequired(previousValue);
    }

    // ─── Private Helpers ───

    private async Task NavigateCalendarToMonth(ILocator calendar, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);
        var title = calendar.Locator(".e-title");

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
                    await calendar.Locator(".e-prev").ClickWhenStableAsync();
                else
                    await calendar.Locator(".e-next").ClickWhenStableAsync();

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

    private async Task ConfirmSelectionIfRequired(string? previousValue)
    {
        var deadline = DateTime.UtcNow.AddSeconds(8);
        while (DateTime.UtcNow < deadline)
        {
            if (!await RangePopup.IsVisibleAsync() && await WaitForCommittedRange(previousValue, timeoutMs: 500))
                return;

            if (await RangePopup.IsVisibleAsync() && await ApplyButton.CountAsync() > 0)
            {
                if (await TryWaitForApplyButtonReady(timeoutMs: 1000))
                {
                    await TryCommitSelectionWithApply();

                    if (!await RangePopup.IsVisibleAsync())
                    {
                        await WaitForCommittedRange(previousValue, timeoutMs: 1500);
                        if (HasCommittedRange(await Input.InputValueAsync(), previousValue))
                            return;
                    }
                }
            }

            await _page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException(
            $"DateRangePicker '{_componentId}' did not commit a new range selection within the expected time.");
    }

    private async Task<bool> WaitForCommittedRange(string? previousValue, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            var currentValue = await Input.InputValueAsync();
            if (HasCommittedRange(currentValue, previousValue))
                return true;

            await _page.WaitForTimeoutAsync(100);
        }

        return false;
    }

    private async Task<bool> TryWaitForApplyButtonReady(int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (await ApplyButton.IsVisibleAsync() && await ApplyButton.IsEnabledAsync())
                return true;

            await _page.WaitForTimeoutAsync(100);
        }

        return false;
    }

    private async Task TryCommitSelectionWithApply()
    {
        try
        {
            var box = await ApplyButton.BoundingBoxAsync();
            if (box is not null)
            {
                await _page.Mouse.ClickAsync(
                    box.X + (box.Width / 2),
                    box.Y + (box.Height / 2));
                await WaitForPopupToHide(timeoutMs: 1500);
                return;
            }
        }
        catch (PlaywrightException)
        {
            // Syncfusion re-renders the footer while closing; fall back to locator click below.
        }

        try
        {
            await ApplyButton.ClickWithoutScrollingWhenStableAsync();
        }
        catch (TimeoutException)
        {
            // The popup can still be animating or re-rendering around the apply action.
        }
        catch (PlaywrightException)
        {
            // Syncfusion can detach the button while committing the selected range.
        }

        await WaitForPopupToHide(timeoutMs: 1500);
    }

    private async Task WaitForPopupToHide(int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (!await RangePopup.IsVisibleAsync())
                return;

            await _page.WaitForTimeoutAsync(50);
        }
    }

    private static bool HasCommittedRange(string currentValue, string? previousValue)
    {
        if (string.IsNullOrWhiteSpace(currentValue))
            return false;

        return !string.Equals(currentValue, previousValue, StringComparison.Ordinal);
    }
}

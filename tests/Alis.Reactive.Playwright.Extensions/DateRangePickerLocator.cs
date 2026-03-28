using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionDateRangePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DateRangePicker DOM:
///   input#{componentId} (type="text", value format: "startDate - endDate")
///   .e-range-icon inside parent .e-input-group (opens range popup)
///   #{componentId}_popup (range popup — NOT _options!)
///   Two calendars: .e-left-container .e-calendar and .e-right-container .e-calendar
///   Apply button: button.e-apply (enabled after both dates selected)
///
/// Usage:
///   var drp = plan.DateRangePicker(m => m.StayRange);
///
///   // Popup gesture — sets ej2.startDate + ej2.endDate reliably
///   await drp.SelectRange(2026, 3, 21, 2026, 3, 28);
///
///   // Text gesture (may not set ej2 values — prefer SelectRange)
///   await drp.FillAndBlur("03/21/2026 - 03/28/2026");
///
///   // Surface → test asserts
///   await Expect(drp.Input).ToHaveValueAsync("03/21/2026 - 03/28/2026");
/// </summary>
public sealed class DateRangePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DateRangePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The date range input field (displays "startDate - endDate").</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The range icon button that opens the date range popup.</summary>
    public ILocator RangeIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-range-icon");

    /// <summary>The date range popup container (visible after clicking RangeIcon).
    /// NOTE: DateRangePicker uses #{id}_popup, NOT #{id}_options like DatePicker.</summary>
    public ILocator RangePopup => _page.Locator($"#{_componentId}_popup");

    /// <summary>The left calendar in the range popup (shows the earlier month).</summary>
    public ILocator LeftCalendar => RangePopup.Locator(".e-left-container .e-calendar");

    /// <summary>The right calendar in the range popup (shows the later month).</summary>
    public ILocator RightCalendar => RangePopup.Locator(".e-right-container .e-calendar");

    /// <summary>The Apply button (enabled after both start and end dates are selected).</summary>
    public ILocator ApplyButton => RangePopup.Locator("button.e-apply");

    // ─── Gestures — What the User Does ───

    /// <summary>Click and fill the input with a date range value.</summary>
    public async Task Fill(string dateRangeText)
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateRangeText, new() { Delay = 30 });
    }

    /// <summary>Click, select all, and delete.</summary>
    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>Press Tab to leave the field.</summary>
    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>Fill a date range value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string dateRangeText)
    {
        await Fill(dateRangeText);
        await Blur();
    }

    /// <summary>Open the range popup, select start and end dates via calendar clicks, then Apply.
    /// This is the reliable way to set ej2.startDate and ej2.endDate.
    ///
    /// Strategy:
    /// 1. Navigate left calendar to start month, click start day in left calendar
    /// 2. Navigate left calendar so end month appears in either left or right calendar
    /// 3. Click end day in the appropriate calendar
    /// 4. Click Apply to confirm the selection
    /// </summary>
    public async Task SelectRange(int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay)
    {
        await RangeIcon.ClickAsync();
        await RangePopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        // Navigate left calendar to start month and click start day
        await NavigateCalendarToMonth(LeftCalendar, startYear, startMonth);
        await LeftCalendar
            .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{startDay}\")")
            .ClickAsync();

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
                .ClickAsync();
        }
        else if (endTarget == startTarget.AddMonths(1))
        {
            // End is in the right calendar (left + 1)
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickAsync();
        }
        else
        {
            // End is further away — navigate left calendar to (endMonth - 1) so right shows endMonth
            await NavigateCalendarToMonth(LeftCalendar, endTarget.AddMonths(-1).Year,
                endTarget.AddMonths(-1).Month);
            await RightCalendar
                .Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{endDay}\")")
                .ClickAsync();
        }

        // Click Apply to confirm the range selection
        await ApplyButton.ClickAsync();
    }

    // ─── Private Helpers ───

    /// <summary>Navigate a calendar (left or right) to show the target month/year by clicking prev/next.</summary>
    private async Task NavigateCalendarToMonth(ILocator calendar, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);

        for (var i = 0; i < 24; i++) // max 2 years of navigation
        {
            var titleText = await calendar.Locator(".e-title").TextContentAsync() ?? "";

            if (DateTime.TryParseExact(titleText.Trim(), "MMMM yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var current))
            {
                var currentMonth = new DateTime(current.Year, current.Month, 1);
                if (currentMonth == target)
                    return;

                if (target < currentMonth)
                    await calendar.Locator(".e-prev").ClickAsync();
                else
                    await calendar.Locator(".e-next").ClickAsync();

                await _page.WaitForTimeoutAsync(100); // allow calendar animation
            }
        }
    }
}

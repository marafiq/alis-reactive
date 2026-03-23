using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DateTimePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DateTimePicker DOM:
///   input#{componentId} (type="text" with date+time formatting)
///   .e-date-icon inside parent .e-input-group (opens calendar popup)
///   .e-time-icon inside parent .e-input-group (opens time list popup)
///   #{componentId}_options (calendar popup, shared with time list)
///
/// Usage:
///   var dtp = plan.DateTimePicker(m => m.AppointmentAt);
///
///   // Popup gesture — sets ej2.value reliably
///   await dtp.Select(2026, 3, 21, "2:30 PM");
///
///   // Or separately
///   await dtp.SelectDate(2026, 3, 21);
///   await dtp.SelectTime("2:30 PM");
///
///   // Text gesture (may not set ej2.value — prefer Select)
///   await dtp.FillAndBlur("03/21/2026 2:30 PM");
///
///   // Surface → test asserts
///   await Expect(dtp.Input).ToHaveValueAsync("03/21/2026 2:30 PM");
/// </summary>
public sealed class DateTimePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DateTimePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The date-time input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The calendar icon button that opens the date popup.</summary>
    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    /// <summary>The clock icon button that opens the time popup.</summary>
    public ILocator ClockIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-time-icon");

    /// <summary>The popup container — used for both calendar and time list.
    /// SF DateTimePicker reuses #{id}_options for both popups (only one visible at a time).</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    /// <summary>The time list popup wrapper (different ID from the calendar popup).</summary>
    public ILocator TimePopup => _page.Locator($"#{_componentId}_timePopup, #{_componentId}_popup").First;

    // ─── Gestures — What the User Does ───

    /// <summary>Click and fill the input with a date-time value.</summary>
    public async Task Fill(string dateTimeText)
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateTimeText, new() { Delay = 30 });
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

    /// <summary>Fill a date-time value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string dateTimeText)
    {
        await Fill(dateTimeText);
        await Blur();
    }

    /// <summary>Open the calendar popup, navigate to the target month/year, and click the day cell.
    /// This sets the date portion of ej2.value reliably.</summary>
    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(Popup, year, month);

        var dayCell = Popup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickAsync();
    }

    /// <summary>Open the time popup and click the matching time item.
    /// This sets the time portion of ej2.value reliably.
    /// Time items use 30-minute intervals (e.g., "8:00 AM", "8:30 AM").</summary>
    public async Task SelectTime(string timeText)
    {
        await ClockIcon.ClickAsync();
        // Time popup reuses the same #{id}_options element
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var item = Popup.Locator($".e-list-item[data-value='{timeText}']");
        await item.ClickAsync();
    }

    /// <summary>Select both date and time via their popups.
    /// This is the most reliable way to set a full DateTime on the ej2 instance.</summary>
    public async Task Select(int year, int month, int day, string timeText)
    {
        await SelectDate(year, month, day);
        await SelectTime(timeText);
    }

    // ─── Private Helpers ───

    private async Task NavigateToMonth(ILocator popup, int targetYear, int targetMonth)
    {
        var target = new DateTime(targetYear, targetMonth, 1);

        for (var i = 0; i < 24; i++) // max 2 years of navigation
        {
            var titleText = await popup.Locator(".e-title").TextContentAsync() ?? "";

            if (DateTime.TryParseExact(titleText.Trim(), "MMMM yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var current))
            {
                var currentMonth = new DateTime(current.Year, current.Month, 1);
                if (currentMonth == target)
                    return;

                if (target < currentMonth)
                    await popup.Locator(".e-prev").ClickAsync();
                else
                    await popup.Locator(".e-next").ClickAsync();

                await _page.WaitForTimeoutAsync(100); // allow calendar animation
            }
        }
    }
}

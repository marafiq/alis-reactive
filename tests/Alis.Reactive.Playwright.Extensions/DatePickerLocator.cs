using System.Globalization;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionDatePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DatePicker DOM:
///   input#{componentId} (type="text" with date formatting)
///   .e-date-icon inside parent .e-input-group (opens calendar popup)
///   #{componentId}_options (calendar popup)
///
/// Usage:
///   var dp = plan.DatePicker(m => m.BirthDate);
///
///   // Popup gesture — sets ej2.value reliably
///   await dp.SelectDate(2026, 3, 21);
///
///   // Text gesture (may not set ej2.value — prefer SelectDate)
///   await dp.FillAndBlur("03/21/2026");
///
///   // Surface → test asserts
///   await Expect(dp.Input).ToHaveValueAsync("03/21/2026");
/// </summary>
public sealed class DatePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DatePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The date input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The calendar icon button that opens the date popup.</summary>
    public ILocator CalendarIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-date-icon");

    /// <summary>The calendar popup container (visible after clicking CalendarIcon).</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    // ─── Gestures — What the User Does ───

    /// <summary>Click, select all, and type a date value keystroke by keystroke.
    /// PressSequentially, not FillAsync — SF needs real keystroke events to parse dates.</summary>
    public async Task Fill(string dateText)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateText, new() { Delay = 30 });
    }

    /// <summary>Click, select all, and delete.</summary>
    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    /// <summary>Press Tab to leave the field.</summary>
    public async Task Blur() => await Input.PressAsync("Tab");

    /// <summary>Fill a date value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string dateText)
    {
        await Fill(dateText);
        await Blur();
    }

    /// <summary>Open the calendar popup, navigate to the target month/year, and click the day cell.
    /// This is the reliable way to set ej2.value — typed input does NOT always update the instance.</summary>
    public async Task SelectDate(int year, int month, int day)
    {
        await CalendarIcon.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await NavigateToMonth(Popup, year, month);

        var dayCell = Popup.Locator($"td.e-cell:not(.e-other-month) span.e-day:text-is(\"{day}\")");
        await dayCell.ClickWithoutScrollingWhenStableAsync(_page);
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
                    await popup.Locator(".e-prev").ClickWithoutScrollingWhenStableAsync(_page);
                else
                    await popup.Locator(".e-next").ClickWithoutScrollingWhenStableAsync(_page);

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

using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DateRangePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DateRangePicker DOM:
///   input#{componentId} (type="text", value format: "startDate - endDate")
///
/// Usage:
///   var drp = plan.DateRangePicker(m => m.StayRange);
///
///   // Gesture
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
}

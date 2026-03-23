using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DatePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DatePicker DOM:
///   input#{componentId} (type="text" with date formatting)
///
/// Usage:
///   var dp = plan.DatePicker(m => m.BirthDate);
///
///   // Gesture
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

    // ─── Gestures — What the User Does ───

    /// <summary>Click, select all, and type a date value keystroke by keystroke.
    /// PressSequentially, not FillAsync — SF needs real keystroke events to parse dates.</summary>
    public async Task Fill(string dateText)
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(dateText, new() { Delay = 30 });
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

    /// <summary>Fill a date value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string dateText)
    {
        await Fill(dateText);
        await Blur();
    }
}

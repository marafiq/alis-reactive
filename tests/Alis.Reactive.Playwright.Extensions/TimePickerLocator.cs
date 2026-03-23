using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion TimePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF TimePicker DOM:
///   input#{componentId} (type="text" with time formatting)
///
/// Usage:
///   var tp = plan.TimePicker(m => m.CheckInTime);
///
///   // Gesture
///   await tp.FillAndBlur("2:30 PM");
///
///   // Surface → test asserts
///   await Expect(tp.Input).ToHaveValueAsync("2:30 PM");
/// </summary>
public sealed class TimePickerLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal TimePickerLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The time input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    /// <summary>Click and fill the input with a time value.</summary>
    public async Task Fill(string timeText)
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressSequentiallyAsync(timeText, new() { Delay = 30 });
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

    /// <summary>Fill a time value and then blur — triggers change event.</summary>
    public async Task FillAndBlur(string timeText)
    {
        await Fill(timeText);
        await Blur();
    }
}

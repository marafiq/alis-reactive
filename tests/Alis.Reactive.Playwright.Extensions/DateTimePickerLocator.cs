using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DateTimePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF DateTimePicker DOM:
///   input#{componentId} (type="text" with date+time formatting)
///
/// Usage:
///   var dtp = plan.DateTimePicker(m => m.AppointmentAt);
///
///   // Gesture
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
}

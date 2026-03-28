using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionTimePicker.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF TimePicker DOM:
///   input#{componentId} (type="text" with time formatting)
///   .e-time-icon inside parent .e-input-group (opens time popup)
///   #{componentId}_popup (time list popup wrapper)
///
/// Usage:
///   var tp = plan.TimePicker(m => m.CheckInTime);
///
///   // Popup gesture — sets ej2.value reliably
///   await tp.SelectTime("2:30 PM");
///
///   // Text gesture (may not set ej2.value — prefer SelectTime)
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

    /// <summary>The clock icon button that opens the time popup.</summary>
    public ILocator ClockIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-time-icon");

    /// <summary>The time list popup wrapper (visible after clicking ClockIcon).</summary>
    public ILocator TimePopup => _page.Locator($"#{_componentId}_popup");

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

    /// <summary>Open the time popup, find the matching time item, and click it.
    /// This is the reliable way to set ej2.value — typed input does NOT always update the instance.
    /// Time items use 30-minute intervals (e.g., "8:00 AM", "8:30 AM", ..., "11:30 PM").</summary>
    public async Task SelectTime(string timeText)
    {
        await ClockIcon.ClickAsync();
        await TimePopup.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var item = TimePopup.Locator($".e-list-item[data-value='{timeText}']");
        await item.ClickAsync();
    }
}

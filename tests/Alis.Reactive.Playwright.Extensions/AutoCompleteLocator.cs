using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionAutoComplete.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF AutoComplete DOM:
///   span.e-input-group (wrapper)
///     └── input#{componentId} (main input — typed text goes here)
///   div.e-ddl.e-popup (popup — appears on keystroke)
///     └── ul
///         └── li.e-list-item (each suggestion)
///
/// Usage:
///   var ac = scope.AutoComplete("Physician");
///
///   // Gesture
///   await ac.Type("smi");
///   await ac.SelectItem("Dr. Smith");
///
///   // Surface → test asserts
///   await Expect(ac.Input).ToHaveValueAsync("Dr. Smith");
///   await Expect(ac.PopupItems).ToHaveCountAsync(3);
/// </summary>
public sealed class AutoCompleteLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal AutoCompleteLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The text input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The popup for this specific AutoComplete instance.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    /// <summary>All suggestion items in this popup.</summary>
    public ILocator PopupItems => Popup.Locator(".e-list-item");

    /// <summary>A specific suggestion item by visible text.</summary>
    public ILocator PopupItem(string text) => PopupItems.GetByText(text, new() { Exact = true });

    // ─── Gestures — What the User Does ───

    /// <summary>
    /// Type text into the input using sequential key presses.
    /// PressSequentially, not Fill — SF needs real keystroke events for filtering.
    /// </summary>
    public async Task Type(string text, int delayMs = 50)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressSequentiallyAsync(text, new() { Delay = delayMs });
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
    }

    /// <summary>
    /// Clear the input the way a user does. Uses Playwright's native ClearAsync
    /// because the keyboard dance (Meta+A, Backspace, Tab) gets reverted by EJ2
    /// on blur — verified in the live browser.
    /// </summary>
    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.ClearAsync();
    }

    /// <summary>Click a suggestion in the popup by its visible text.</summary>
    public async Task SelectItem(string itemText, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }

    /// <summary>Type partial text, then click a matching suggestion. The full user gesture.</summary>
    public async Task TypeAndSelect(string searchText, string itemText, int delayMs = 50)
    {
        await Type(searchText, delayMs);
        await SelectItem(itemText);
    }

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    /// <summary>Press Tab to leave the field.</summary>
    public async Task Blur() => await Input.PressAsync("Tab");
}

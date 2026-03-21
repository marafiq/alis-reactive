using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright locator for Syncfusion AutoComplete component.
/// Every action is a real user gesture — no ej2 API, no programmatic manipulation.
///
/// SF AutoComplete DOM:
///   span.e-input-group (wrapper)
///     └── input#{componentId} (main input — accepts typed text directly)
///   div.e-ddl.e-popup (popup — appears on type/focus)
///     └── ul
///         └── li.e-list-item (each suggestion)
///
/// Usage:
///   var physician = scope.AutoComplete("Physician");
///   await physician.Type("smi");
///   await physician.ExpectPopupVisible();
///   await physician.SelectItem("Dr. Smith");
///   await physician.ExpectInputText("Dr. Smith");
/// </summary>
public class AutoCompleteLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public AutoCompleteLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The raw input element.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The SF popup container.</summary>
    public ILocator Popup => _page.Locator(".e-ddl.e-popup");

    /// <summary>All visible popup list items.</summary>
    public ILocator PopupItems => _page.Locator(".e-ddl.e-popup .e-list-item");

    /// <summary>A specific popup item by visible text.</summary>
    public ILocator PopupItem(string text) =>
        _page.Locator(".e-ddl.e-popup .e-list-item").Filter(new() { HasText = text });

    // ─── Actions — Real User Gestures Only ───

    /// <summary>
    /// Type text into the AutoComplete input using sequential key presses.
    /// PressSequentially (not Fill) — SF needs real keystroke events to trigger filtering.
    /// </summary>
    public async Task Type(string text, int delayMs = 50)
    {
        await Input.ClickAsync();
        await Input.PressSequentiallyAsync(text, new() { Delay = delayMs });
    }

    /// <summary>
    /// Clear the input text — triple-click to select all, then delete.
    /// Real user gesture: select all → backspace.
    /// </summary>
    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    /// <summary>Click a popup item by its visible text.</summary>
    public async Task SelectItem(string itemText, int timeoutMs = 5000)
    {
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { Timeout = timeoutMs });
        await item.ClickAsync();
    }

    /// <summary>
    /// Type text and select a matching popup item — the complete user gesture.
    /// This is what a real user does: type partial text → see suggestions → click one.
    /// </summary>
    public async Task TypeAndSelect(string searchText, string itemText, int delayMs = 50)
    {
        await Type(searchText, delayMs);
        await SelectItem(itemText);
    }

    /// <summary>Click the input to give it focus.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>Press Tab to blur out of the input.</summary>
    public async Task Blur() => await Input.PressAsync("Tab");

    // ─── Assertions — What the User Sees ───

    /// <summary>Assert the popup is visible with at least one item.</summary>
    public async Task ExpectPopupVisible(int timeoutMs = 5000)
    {
        await Expect(PopupItems.First).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the popup is not visible.</summary>
    public async Task ExpectPopupHidden(int timeoutMs = 5000)
    {
        await Expect(Popup).Not.ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert a specific item exists in the popup.</summary>
    public async Task ExpectPopupContains(string itemText, int timeoutMs = 5000)
    {
        await Expect(PopupItem(itemText)).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the popup has exactly N items.</summary>
    public async Task ExpectPopupCount(int count, int timeoutMs = 5000)
    {
        await Expect(PopupItems).ToHaveCountAsync(count, new() { Timeout = timeoutMs });
    }

    /// <summary>
    /// Assert the input shows expected text — what the user sees in the text field.
    /// This is the observable state, not ej2 internal value.
    /// </summary>
    public async Task ExpectInputText(string expectedText, int timeoutMs = 5000)
    {
        await Expect(Input).ToHaveValueAsync(expectedText, new() { Timeout = timeoutMs });
    }

    /// <summary>Assert the input is empty — no text visible to the user.</summary>
    public async Task ExpectEmpty()
    {
        await Expect(Input).ToHaveValueAsync("");
    }

    /// <summary>Assert the input is visible and enabled.</summary>
    public async Task ExpectVisible()
    {
        await Expect(Input).ToBeVisibleAsync();
        await Expect(Input).ToBeEnabledAsync();
    }

    /// <summary>Assert the input is disabled.</summary>
    public async Task ExpectDisabled()
    {
        await Expect(Input).ToBeDisabledAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) =>
        Assertions.Expect(locator);
}

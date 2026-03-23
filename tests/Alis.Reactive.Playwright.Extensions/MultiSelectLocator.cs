using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion MultiSelect.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF MultiSelect DOM:
///   input#{componentId} (hidden)
///   span.e-multi-select-wrapper (visible wrapper — click to open)
///   div#{componentId}_popup (popup when open)
///     └── .e-list-item (each option)
///
/// Usage:
///   var ms = plan.MultiSelect(m => m.Allergies);
///
///   // Gesture — select multiple items
///   await ms.SelectItems("Penicillin", "Sulfa");
///
///   // Surface → test asserts
///   await Expect(ms.PopupItem("Penicillin")).ToHaveClassAsync(/e-active/);
/// </summary>
public sealed class MultiSelectLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal MultiSelectLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The visible control wrapper (click to open popup).</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::div[contains(@class,'e-input-group')]");

    /// <summary>
    /// The popup container. SF MultiSelect creates the popup with a shortened ID
    /// (property name only, e.g., "DietaryRestrictions_popup" not the full scope).
    /// Extract the property name from the end of the component ID.
    /// </summary>
    public ILocator Popup => _page.Locator($"#{_componentId.Split("__").Last()}_popup");

    /// <summary>All popup list items.</summary>
    public ILocator PopupItems => Popup.Locator(".e-list-item");

    /// <summary>A specific popup item by its visible text.</summary>
    public ILocator PopupItem(string text) =>
        PopupItems.Filter(new() { HasText = text });

    // ─── Gestures — What the User Does ───

    /// <summary>Click the wrapper to focus and open the popup.</summary>
    public async Task Open()
    {
        await Wrapper.ClickAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>Open the popup, find the item by text, and click it.</summary>
    public async Task SelectItem(string itemText)
    {
        await Open();
        await Popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true }).ClickAsync();
    }

    /// <summary>Select multiple items. SF MultiSelect may close popup after each click,
    /// so we re-open the popup for each subsequent item.</summary>
    public async Task SelectItems(params string[] itemTexts)
    {
        foreach (var text in itemTexts)
        {
            // Click wrapper to open/re-open popup
            await Wrapper.ClickAsync();
            await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await Popup.Locator(".e-list-item").GetByText(text, new() { Exact = true }).ClickAsync();
        }
        // Close popup if still open
        if (await Popup.IsVisibleAsync())
            await _page.Keyboard.PressAsync("Escape");
    }

    /// <summary>Press Escape to close the popup and blur.</summary>
    public async Task Blur() => await _page.Keyboard.PressAsync("Escape");
}

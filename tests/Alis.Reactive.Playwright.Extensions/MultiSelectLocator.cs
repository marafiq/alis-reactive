using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion MultiSelect.
/// Provides gestures (what a user does) and surfaces (what a user sees).
/// Does NOT provide assertions — the test decides what to verify.
///
/// SF MultiSelect DOM:
///   input#{componentId} (hidden)
///   div.e-multi-select-wrapper (visible wrapper — click to open)
///   div#{shortPropertyName}_popup (popup when open)
///     └── .e-list-item (each option)
///
/// CRITICAL: SF MultiSelect fires the `change` event on BLUR (when focus leaves
/// the component), NOT on individual item selection. After selecting items,
/// you must click outside the component to trigger the change event.
///
/// Usage:
///   var ms = scope.MultiSelect("Allergies");
///
///   // Gesture — select item (opens popup, clicks item, blurs to fire change)
///   await ms.SelectItem("Penicillin");
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

    /// <summary>The visible control wrapper (click to open popup).
    /// SF MultiSelect: clicking .e-multi-select-wrapper opens the popup.
    /// Using the inner wrapper (not the outer e-input-group) ensures the click
    /// targets the SF event handler area.</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::div[contains(@class,'e-multi-select-wrapper')]");

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

    /// <summary>Open the popup, find the item by text, click it, then blur to fire change event.
    /// SF MultiSelect fires the `change` event on blur, not on item click.
    /// The blur is achieved by clicking the document body outside the component.</summary>
    public async Task SelectItem(string itemText)
    {
        await Open();
        await Popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true }).ClickAsync();
        // Wait for popup to close (closePopupOnSelect: true in Box mode)
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        // Click outside the component to blur — this triggers the SF change event
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
    }

    /// <summary>Select multiple items. SF MultiSelect closes popup after each click
    /// (closePopupOnSelect: true), so we re-open for each subsequent item.
    /// After all items are selected, blurs to fire the change event.</summary>
    public async Task SelectItems(params string[] itemTexts)
    {
        foreach (var text in itemTexts)
        {
            // Click wrapper to open/re-open popup
            await Wrapper.ClickAsync();
            await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await Popup.Locator(".e-list-item").GetByText(text, new() { Exact = true }).ClickAsync();
            // Wait for popup to close after selection
            await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        }
        // Click outside the component to blur — triggers the SF change event
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
    }

    /// <summary>Press Escape to close the popup and blur.</summary>
    public async Task Blur() => await _page.Keyboard.PressAsync("Escape");
}

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
    /// All popup list items. SF MultiSelect creates the popup with a shortened ID
    /// (property name only), so we use a general visible-popup selector.
    /// </summary>
    public ILocator PopupItems => _page.Locator("[id$='_popup'] .e-list-item");

    /// <summary>A specific popup item by its visible text.</summary>
    public ILocator PopupItem(string text) =>
        PopupItems.Filter(new() { HasText = text });

    // ─── Gestures — What the User Does ───

    /// <summary>Click the wrapper to focus and open the popup.</summary>
    public async Task Open() => await Wrapper.ClickAsync();

    /// <summary>Open the popup, find the item by text, and click it.</summary>
    public async Task SelectItem(string itemText, int timeoutMs = 5000)
    {
        await Open();
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { Timeout = timeoutMs });
        await item.ClickAsync();
    }

    /// <summary>Select multiple items. Opens popup, clicks each, then closes.</summary>
    public async Task SelectItems(params string[] itemTexts)
    {
        foreach (var text in itemTexts)
        {
            // Re-open popup for each item (SF may briefly close between selections)
            await Open();
            var item = PopupItem(text);
            await item.WaitForAsync(new() { Timeout = 5000 });
            await item.ClickAsync();
        }
        await _page.Keyboard.PressAsync("Escape");
    }

    /// <summary>Press Escape to close the popup and blur.</summary>
    public async Task Blur() => await _page.Keyboard.PressAsync("Escape");
}

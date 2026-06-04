using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Selects through the popup and blurs because Syncfusion MultiSelect raises <c>change</c> on blur.
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

    /// <summary>Inner wrapper; clicking the outer input group can miss Syncfusion's handler.</summary>
    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::div[contains(@class,'e-multi-select-wrapper')]");

    /// <summary>Popup uses the property-name suffix, for example <c>DietaryRestrictions_popup</c>.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId.Split("__").Last()}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator PopupItem(string text) =>
        PopupItems.Filter(new() { HasText = text });

    public async Task Open()
    {
        await Wrapper.ClickAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>Selects one item and blurs so Syncfusion raises <c>change</c>.</summary>
    public async Task SelectItem(string itemText)
    {
        await Open();
        await Popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true }).ClickAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
    }

    /// <summary>Selects multiple items, reopening after each Box-mode popup close, then blurs.</summary>
    public async Task SelectItems(params string[] itemTexts)
    {
        foreach (var text in itemTexts)
        {
            await Open();
            await Popup.Locator(".e-list-item").GetByText(text, new() { Exact = true }).ClickAsync();
            await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        }
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
    }

    public async Task Blur() => await _page.Keyboard.PressAsync("Escape");
}

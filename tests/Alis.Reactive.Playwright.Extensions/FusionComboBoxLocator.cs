using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionComboBox.
/// </summary>
public sealed class FusionComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered ComboBox input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The Syncfusion wrapper around the input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    /// <summary>The popup for this ComboBox instance.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    /// <summary>The popup items for this ComboBox instance.</summary>
    public ILocator PopupItems => Popup.Locator(".e-list-item");

    /// <summary>A popup item by exact visible text.</summary>
    public ILocator PopupItem(string text) => PopupItems.GetByText(text, new() { Exact = true });

    /// <summary>Selects an item from the currently open popup.</summary>
    public async Task SelectItem(string text, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var item = PopupItem(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }
}

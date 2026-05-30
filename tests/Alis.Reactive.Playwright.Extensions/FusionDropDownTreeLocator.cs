using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionDropDownTree.
/// </summary>
public sealed class FusionDropDownTreeLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownTreeLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered DropDownTree input.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The Syncfusion wrapper around the input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    /// <summary>The popup for this DropDownTree instance.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    /// <summary>The rendered tree inside the popup.</summary>
    public ILocator Tree => _page.Locator($"#{_componentId}_tree");

    /// <summary>A tree item text span by exact visible text.</summary>
    public ILocator TreeItemText(string text) =>
        Tree.Locator(".e-list-text").GetByText(text, new() { Exact = true });

    /// <summary>Selects an item from the currently open popup.</summary>
    public async Task SelectItem(string text, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var itemText = TreeItemText(text);
        await itemText.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var row = itemText.Locator("xpath=ancestor::li[contains(@class,'e-list-item')][1]").Locator(".e-fullrow").First;
        await row.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }
}

using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionDropDownTreeLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownTreeLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    /// <summary>DropDownTree uses <c>#{id}_options</c> for its popup.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    public ILocator Tree => _page.Locator($"#{_componentId}_tree");

    public ILocator TreeItemText(string text) =>
        Tree.Locator(".e-list-text").GetByText(text, new() { Exact = true });

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

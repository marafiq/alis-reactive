using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>
/// A Syncfusion DropDownTree: the input <c>#{componentId}</c> that shows the chosen node's text, the box
/// around it a user clicks, the popup <c>#{componentId}_options</c> it opens, and the tree
/// <c>#{componentId}_tree</c> inside it.
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

    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>
    /// The input group around the input: the box a pointer lands on, since the theme lets the pointer pass
    /// through the input (<c>.e-ddt .e-dropdowntree { pointer-events: none }</c>), and the element whose
    /// mouseup opens and closes the tree.
    /// </summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    /// <summary>The popup; it exists only once the tree has opened.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    public ILocator Tree => _page.Locator($"#{_componentId}_tree");

    public ILocator TreeItemText(string text) =>
        Tree.Locator(".e-list-text").GetByText(text, new() { Exact = true });

    /// <summary>Clicks the box, which opens the closed tree, and waits for the popup.</summary>
    public async Task Open(int timeoutMs = 15000)
    {
        await Wrapper.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
    }

    /// <summary>Clicks the row of the node with this text in the open tree; choosing a node closes the popup.</summary>
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

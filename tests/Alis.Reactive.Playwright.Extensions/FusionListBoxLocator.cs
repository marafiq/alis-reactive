using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>A Syncfusion ListBox by its component ID: its items and which of them are selected.</summary>
public sealed class FusionListBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}_parent");

    /// <summary>List item showing this text.</summary>
    public ILocator Item(string text) => Root
        .Locator("li.e-list-item")
        .Filter(new() { HasTextString = text })
        .First;

    /// <summary>The items the list shows as selected.</summary>
    public ILocator SelectedItems => Root.Locator("li.e-list-item.e-selected");

    /// <summary>Clicks an item: it becomes the only one selected.</summary>
    public async Task ClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
    }

    /// <summary>Ctrl-clicks (Command-clicks on a Mac) an item: it joins the selection, or leaves it if selected.</summary>
    public async Task CtrlClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickAsync(new() { Modifiers = new[] { KeyboardModifier.ControlOrMeta }, Timeout = timeoutMs });
    }
}

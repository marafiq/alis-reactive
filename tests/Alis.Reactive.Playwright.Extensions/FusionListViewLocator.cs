using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>A Syncfusion ListView by its component ID: its items, which are selected and which are checked.</summary>
public sealed class FusionListViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>List item matching exact visible text.</summary>
    public ILocator Item(string text) =>
        Root.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::li[contains(@class,'e-list-item')][1]");

    /// <summary>The items the list shows as selected (checked items of a checkbox list included).</summary>
    public ILocator SelectedItems => Root.Locator("li.e-list-item.e-active");

    /// <summary>The check mark of the item with this text, present only while the item is checked.</summary>
    public ILocator CheckMark(string text) => Item(text).Locator(".e-check");

    /// <summary>Every check mark in a checkbox list.</summary>
    public ILocator CheckMarks => Root.Locator("li.e-list-item .e-check");

    public async Task ClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
    }
}

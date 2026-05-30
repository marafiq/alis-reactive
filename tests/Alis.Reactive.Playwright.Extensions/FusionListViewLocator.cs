using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionListView.
/// </summary>
public sealed class FusionListViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered ListView root.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>A list item by exact visible text.</summary>
    public ILocator Item(string text) =>
        Root.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::li[contains(@class,'e-list-item')][1]");

    /// <summary>The checked icon for a list item.</summary>
    public ILocator CheckedIcon(string text) => Item(text).Locator(".e-check");

    /// <summary>Selects or toggles an item through the browser UI.</summary>
    public async Task ClickItem(string text, int timeoutMs = 15000)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
    }

    public async Task<bool> IsSelected(string text)
    {
        var className = await Item(text).GetAttributeAsync("class");
        return Regex.IsMatch(className ?? string.Empty, @"\be-active\b");
    }
}

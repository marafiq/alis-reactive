using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Playwright gestures and surfaces for FusionListView tests.
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

    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>A list item by exact visible text.</summary>
    public ILocator Item(string text) =>
        Root.GetByText(text, new() { Exact = true })
            .Locator("xpath=ancestor::li[contains(@class,'e-list-item')][1]");

    public ILocator CheckedIcon(string text) => Item(text).Locator(".e-check");

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

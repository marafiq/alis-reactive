using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionListBox.
/// </summary>
public sealed class FusionListBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionListBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered ListBox wrapper.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}_parent");

    /// <summary>A list item by exact visible text.</summary>
    public ILocator Item(string text) => Root
        .Locator("li.e-list-item")
        .Filter(new() { HasTextString = text })
        .First;

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
        return Regex.IsMatch(className ?? string.Empty, @"\be-(selected|active)\b");
    }

    public async Task<bool> IsDisabled(string text)
    {
        var className = await Item(text).GetAttributeAsync("class");
        return Regex.IsMatch(className ?? string.Empty, @"\be-disabled\b");
    }
}

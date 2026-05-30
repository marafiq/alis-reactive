using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionBreadcrumb.
/// </summary>
public sealed class FusionBreadcrumbLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionBreadcrumbLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered breadcrumb root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The current breadcrumb item rendered with aria-current.</summary>
    public ILocator CurrentItem => Root.Locator("[aria-current='page']");

    /// <summary>Locates a clickable breadcrumb link by its visible text.</summary>
    public ILocator Link(string text) => Root.GetByRole(AriaRole.Link, new() { Name = text });

    /// <summary>Clicks a breadcrumb link through the browser UI.</summary>
    public async Task ClickLink(string text, int timeoutMs = 15000)
    {
        var link = Link(text);
        await link.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await link.ClickWhenStableAsync(_page, timeoutMs);
    }

    /// <summary>Returns the current visible breadcrumb text.</summary>
    public async Task<string> CurrentText()
    {
        return await CurrentItem.TextContentAsync() ?? string.Empty;
    }
}

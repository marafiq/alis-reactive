using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionBreadcrumbLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionBreadcrumbLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>Locates the breadcrumb item marked as the current page.</summary>
    public ILocator CurrentItem => Root.Locator("[aria-current='page']");

    /// <summary>Locates a clickable breadcrumb link by its visible text.</summary>
    public ILocator Link(string text) => Root.GetByRole(AriaRole.Link, new() { Name = text });

    public async Task ClickLink(string text, int timeoutMs = 15000)
    {
        var link = Link(text);
        await link.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await link.ClickWhenStableAsync(_page, timeoutMs);
    }

    public async Task<string> CurrentText()
    {
        return await CurrentItem.TextContentAsync() ?? string.Empty;
    }
}

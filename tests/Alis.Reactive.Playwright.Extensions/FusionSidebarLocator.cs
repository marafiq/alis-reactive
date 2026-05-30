using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionSidebar.
/// </summary>
public sealed class FusionSidebarLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSidebarLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered sidebar root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The open-state root element.</summary>
    public ILocator OpenRoot => _page.Locator($"#{_componentId}.e-open");

    /// <summary>The closed-state root element.</summary>
    public ILocator ClosedRoot => _page.Locator($"#{_componentId}.e-close");

    /// <summary>The sidebar text content.</summary>
    public async Task<string> Text() => await Root.TextContentAsync() ?? string.Empty;
}

using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionContextMenu.
/// </summary>
public sealed class FusionContextMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionContextMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered context menu root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The sandbox target area that drives the context menu.</summary>
    public ILocator Target => _page.Locator("#resident-context-target");

    /// <summary>The programmatic open button on the sandbox page.</summary>
    public ILocator OpenButton => _page.Locator("#open-context-menu-btn");

    /// <summary>The programmatic close button on the sandbox page.</summary>
    public ILocator CloseButton => _page.Locator("#close-context-menu-btn");

    /// <summary>Menu item by exact visible text.</summary>
    public ILocator Item(string text) => _page.GetByRole(AriaRole.Menuitem, new() { Name = text });
}

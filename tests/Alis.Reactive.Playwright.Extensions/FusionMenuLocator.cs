using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionMenu.
/// </summary>
public sealed class FusionMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered menu root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>Menu item by exact visible text.</summary>
    public ILocator Item(string text) => Root.GetByText(text, new() { Exact = true });

    /// <summary>The open-menu button on the sandbox page.</summary>
    public ILocator OpenButton => _page.Locator("#open-menu-btn");

    /// <summary>The close-menu button on the sandbox page.</summary>
    public ILocator CloseButton => _page.Locator("#close-menu-btn");
}

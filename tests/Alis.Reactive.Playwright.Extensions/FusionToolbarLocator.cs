using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionToolbar.
/// </summary>
public sealed class FusionToolbarLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionToolbarLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered toolbar root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The save toolbar item.</summary>
    public ILocator SaveItem => _page.Locator("#toolbar-save");

    /// <summary>The delete toolbar item.</summary>
    public ILocator DeleteItem => _page.Locator("#toolbar-delete");

    /// <summary>The disabled-state root element.</summary>
    public ILocator DisabledRoot => _page.Locator($"#{_componentId}.e-disabled");
}

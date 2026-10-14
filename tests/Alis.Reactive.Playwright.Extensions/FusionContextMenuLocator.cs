using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>A Syncfusion ContextMenu: its top list, the lists it has open, and an item by its text.</summary>
public sealed class FusionContextMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionContextMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The menu's top list; it shows while the menu is open.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The menu's wrapper, which holds the top list and every submenu list the menu has open.</summary>
    public ILocator Lists => Root.Locator("xpath=..");

    /// <summary>The item with this text, in the top list or in an open submenu.</summary>
    public ILocator Item(string text) => Lists.GetByRole(AriaRole.Menuitem, new() { Name = text, Exact = true });
}

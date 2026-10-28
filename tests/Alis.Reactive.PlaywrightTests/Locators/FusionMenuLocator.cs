using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>A Syncfusion Menu: its top list, the hamburger icon that opens and closes it, and an item by its text.</summary>
public sealed class FusionMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The menu's top list; a hamburger menu hides it while the menu is closed.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The menu's wrapper: its hamburger header, its top list and every section list it has open.</summary>
    public ILocator Wrapper => Root.Locator("xpath=..");

    /// <summary>The icon in a hamburger menu's header that opens and closes the menu.</summary>
    public ILocator HamburgerIcon => Wrapper.Locator(".e-menu-header .e-menu-icon");

    /// <summary>The item with this text, in the top list or in an open section; a Menu labels each item with its text.</summary>
    public ILocator Item(string text) => Wrapper.GetByLabel(text, new() { Exact = true });

    /// <summary>
    /// Taps the item's own row. A hamburger menu opens a section's list inside the section's item, so a tap on
    /// the middle of an open section would land on one of its pages.
    /// </summary>
    public Task Tap(string text) => Item(text).ClickAsync(new() { Position = new() { X = 12, Y = 12 } });
}

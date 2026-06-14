using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locators for a rendered Syncfusion Sidebar panel and the navigation it holds.
/// The Sidebar stamps <c>e-open</c>/<c>e-close</c> on its controlled root element,
/// which is the user-visible slide-out state.
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

    /// <summary>The Sidebar's controlled root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The root while the panel is slid into view.</summary>
    public ILocator OpenRoot => _page.Locator($"#{_componentId}.e-open");

    /// <summary>The root while the panel is tucked away.</summary>
    public ILocator ClosedRoot => _page.Locator($"#{_componentId}.e-close");

    /// <summary>A navigation link inside the panel, matched by its visible text.</summary>
    public ILocator NavLink(string text) =>
        Root.GetByRole(AriaRole.Link, new() { Name = text });
}

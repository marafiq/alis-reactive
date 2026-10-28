using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locates a rendered Syncfusion Toolbar and its command items the way a user sees them.
/// Toolbar items render with the developer-chosen item id as their element id.
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

    /// <summary>The rendered toolbar root.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>
    /// The toolbar root only when it carries Syncfusion's disabled state class.
    /// EJ2 Toolbar.disable(true) adds <c>e-overlay</c> to the root element
    /// (ej2-navigations toolbar.js: CLS_DISABLE = 'e-overlay').
    /// </summary>
    public ILocator DisabledRoot => _page.Locator($"#{_componentId}.e-overlay");

    /// <summary>A toolbar command item by its developer-chosen id.</summary>
    public ILocator Command(string itemId) => _page.Locator($"#{itemId}");

    /// <summary>Clicks a toolbar command the way a resident would.</summary>
    public async Task ClickCommand(string itemId) => await Command(itemId).ClickAsync();
}
